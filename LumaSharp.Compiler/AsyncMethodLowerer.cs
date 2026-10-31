namespace LumaSharp.Compiler;

internal static class AsyncMethodLowerer
{
    public static IReadOnlyList<CompilationUnit> Lower(IReadOnlyList<CompilationUnit> units, DiagnosticBag diagnostics)
    {
        var nextGeneratedMethod = 0;
        var nextAwaitBatchGroup = 0;
        var awaitBatchGroups = new Dictionary<AwaitBatchExpression, int>(ReferenceEqualityComparer.Instance);
        string? currentAwaitFrame = null;
        var appendAwaitObserverFrame = false;
        return units.Select(unit => unit with { Types = unit.Types.Select(LowerType).ToArray() }).ToArray();

        TypeDeclaration LowerType(TypeDeclaration type)
        {
            var members = new List<MemberDeclaration>();
            foreach (var member in type.Members)
            {
                if (member is MethodDeclaration { IsAsync: true } asyncMethod)
                {
                    if (asyncMethod.Body is null)
                    {
                        ValidateAsyncMethod(type, asyncMethod);
                        members.Add(asyncMethod with { ReturnType = GetTaskType(asyncMethod), IsAsync = false });
                    }
                    else
                    {
                        var (wrapper, bodyMethod) = LowerMethod(type, asyncMethod);
                        members.Add(wrapper);
                        members.Add(bodyMethod);
                    }
                }
                else if (member is MethodDeclaration { Body: not null } method)
                    members.Add(method with { Body = (BlockStatement)RewriteStatement(method.Body, inAsyncMethod: false) });
                else
                    members.Add(member);
            }
            return type with { Members = members };
        }

        (MethodDeclaration Wrapper, MethodDeclaration Body) LowerMethod(TypeDeclaration owner, MethodDeclaration method)
        {
            var location = method.Location;
            ValidateAsyncMethod(owner, method);
            if (method.Body is null)
                return (method with { IsAsync = false }, method);

            var generatedName = "__hidden_" + method.Name + "_async_body_" + nextGeneratedMethod++;
            var ownerName = string.IsNullOrEmpty(owner.Namespace) ? owner.Name : owner.Namespace + "." + owner.Name;
            var parameterTypes = string.Join(", ", method.Parameters.Select(parameter => parameter.TypeName));
            var sourceFrame = $"at {ownerName}.{method.Name}({parameterTypes})\n";
            currentAwaitFrame = sourceFrame;
            var asyncBody = (BlockStatement)RewriteStatement(method.Body, inAsyncMethod: true);
            currentAwaitFrame = null;
            var bodyMethod = method with
            {
                Name = generatedName,
                Body = asyncBody,
                IsAsync = false,
                IsVirtual = false,
                IsOverride = false,
                Accessibility = "private",
                IsCompilerGenerated = true
            };

            var taskName = "$async_task_" + nextGeneratedMethod++;
            var taskType = GetTaskType(method);
            var callerStackTrace = new CallExpression(
                new MemberAccessExpression(
                    new AllocationExpression("System.Diagnostics.StackTrace", [], null, false, location,
                        ConstructorArguments: [new LiteralExpression("0", location)]),
                    "ToString", location),
                [], location);
            var stackTrace = new BinaryExpression(
                new LiteralExpression(QuoteStringLiteral(sourceFrame), location), "+", callerStackTrace, location);
            var taskAllocation = new AllocationExpression(taskType, [], null, false, location,
                ConstructorArguments: [stackTrace]);
            var taskLocal = new VariableStatement(taskName, true, taskType, taskAllocation, location);

            Expression helperTarget = method.IsStatic
                ? new MemberAccessExpression(new NameExpression(ownerName, location), generatedName, location)
                : new MemberAccessExpression(new ThisExpression(location), generatedName, location);
            var helperCall = new CallExpression(helperTarget,
                method.Parameters.Select(parameter => (Expression)new NameExpression(parameter.Name, parameter.Location)).ToArray(),
                location, TypeArguments: method.TypeParameters is { Count: > 0 } ? method.TypeParameters : null);
            Statement callbackStatement = method.ReturnType == "void"
                ? new ExpressionStatement(helperCall, location)
                : new ExpressionStatement(new CallExpression(
                    new MemberAccessExpression(new NameExpression(taskName, location), "SetResult", location),
                    [helperCall], location), location);
            var callback = new LambdaExpression([], null, new BlockStatement([callbackStatement], location), location);
            var setCallback = new ExpressionStatement(new CallExpression(
                new MemberAccessExpression(new NameExpression(taskName, location), "SetCallback", location),
                [callback], location), location);
            var queueTask = new ExpressionStatement(new CallExpression(
                new MemberAccessExpression(new NameExpression("System.Threading.Tasks.ThreadPool", location), "QueueTask", location),
                [new NameExpression(taskName, location)], location), location);
            var wrapper = method with
            {
                ReturnType = taskType,
                Body = new BlockStatement([taskLocal, setCallback, queueTask, new ReturnStatement(new NameExpression(taskName, location), location)], location),
                IsAsync = false
            };
            return (wrapper, bodyMethod);
        }

        void ValidateAsyncMethod(TypeDeclaration owner, MethodDeclaration method)
        {
            var location = method.Location;
            if (method.ReturnsByRef)
                diagnostics.Add("LS0105", DiagnosticSeverity.Error, "An async method cannot return by reference.", location);
            if (method.ReturnType is "Task" or "System.Threading.Tasks.Task" ||
                method.ReturnType.StartsWith("Task<", StringComparison.Ordinal) ||
                method.ReturnType.StartsWith("System.Threading.Tasks.Task<", StringComparison.Ordinal))
                diagnostics.Add("LS0105", DiagnosticSeverity.Error, "An async method declares its result type directly; Task is added implicitly.", location);
            if (method.Parameters.Any(parameter => parameter.PassingMode.Length > 0 || parameter.IsScoped))
                diagnostics.Add("LS0105", DiagnosticSeverity.Error, "Async methods cannot have ref, in, out, or scope-bound parameters.", location);
            if (owner.Kind == "struct" && method.IsMutating && !method.IsStatic)
                diagnostics.Add("LS0105", DiagnosticSeverity.Error, "A mutating struct instance method cannot be async because its receiver cannot safely outlive the call.", location);
        }

        static string GetTaskType(MethodDeclaration method) => method.ReturnType == "void"
            ? "System.Threading.Tasks.Task"
            : $"System.Threading.Tasks.Task<{method.ReturnType}>";

        static string QuoteStringLiteral(string value)
        {
            var builder = new System.Text.StringBuilder(value.Length + 2);
            builder.Append('"');
            foreach (var character in value)
            {
                builder.Append(character switch
                {
                    '\\' => "\\\\",
                    '"' => "\\\"",
                    '\n' => "\\n",
                    '\r' => "\\r",
                    '\t' => "\\t",
                    '\0' => "\\0",
                    _ => character.ToString()
                });
            }
            builder.Append('"');
            return builder.ToString();
        }

        Statement RewriteStatement(Statement statement, bool inAsyncMethod) => statement switch
        {
            BlockStatement block => block with { Statements = block.Statements.Select(child => RewriteStatement(child, inAsyncMethod)).ToArray() },
            ReturnStatement returned => returned with { Value = RewriteOptional(returned.Value, inAsyncMethod) },
            VariableStatement variable => variable with { Initializer = RewriteOptional(variable.Initializer, inAsyncMethod) },
            VariableDeclarationStatement declaration => declaration with { Variables = declaration.Variables.Select(variable => (VariableStatement)RewriteStatement(variable, inAsyncMethod)).ToArray() },
            MultiAssignmentStatement assignment => assignment with
            {
                Targets = assignment.Targets.Select(expression => RewriteExpression(expression, inAsyncMethod)).ToArray(),
                Values = assignment.Values.Select(expression => RewriteExpression(expression, inAsyncMethod)).ToArray()
            },
            ExpressionStatement expression => expression with { Value = RewriteExpression(expression.Value, inAsyncMethod) },
            IfStatement conditional => conditional with
            {
                Condition = RewriteExpression(conditional.Condition, inAsyncMethod),
                Then = RewriteStatement(conditional.Then, inAsyncMethod),
                Else = conditional.Else is null ? null : RewriteStatement(conditional.Else, inAsyncMethod)
            },
            WhileStatement loop => loop with { Condition = RewriteExpression(loop.Condition, inAsyncMethod), Body = RewriteStatement(loop.Body, inAsyncMethod) },
            DoWhileStatement loop => loop with { Body = RewriteStatement(loop.Body, inAsyncMethod), Condition = RewriteExpression(loop.Condition, inAsyncMethod) },
            ForStatement loop => loop with
            {
                Initializer = loop.Initializer is null ? null : RewriteStatement(loop.Initializer, inAsyncMethod),
                Condition = RewriteOptional(loop.Condition, inAsyncMethod),
                Iterators = loop.Iterators.Select(expression => RewriteExpression(expression, inAsyncMethod)).ToArray(),
                Body = RewriteStatement(loop.Body, inAsyncMethod)
            },
            ForInStatement loop => loop with { Collection = RewriteExpression(loop.Collection, inAsyncMethod), Body = RewriteStatement(loop.Body, inAsyncMethod) },
            SwitchStatement selection => selection with
            {
                Value = RewriteExpression(selection.Value, inAsyncMethod),
                Sections = selection.Sections.Select(section =>
                {
                    var statements = section.Statements.Select(child => RewriteStatement(child, inAsyncMethod)).ToArray();
                    return section with
                    {
                        Labels = section.Labels.Select(label => label with
                        {
                            Value = RewriteOptional(label.Value, inAsyncMethod),
                            Guard = RewriteOptional(label.Guard, inAsyncMethod)
                        }).ToArray(),
                        Statements = statements,
                        CaptureScope = new BlockStatement(statements, section.At)
                    };
                }).ToArray()
            },
            TryStatement attempt => RewriteTry(attempt, inAsyncMethod),
            ThrowStatement thrown => thrown with { Value = RewriteOptional(thrown.Value, inAsyncMethod) },
            OverflowContextStatement overflow => overflow with { Body = RewriteStatement(overflow.Body, inAsyncMethod) },
            _ => statement
        };

        TryStatement RewriteTry(TryStatement attempt, bool inAsyncMethod)
        {
            var previousAppendAwaitObserverFrame = appendAwaitObserverFrame;
            appendAwaitObserverFrame = previousAppendAwaitObserverFrame || attempt.Catches.Count > 0;
            var body = (BlockStatement)RewriteStatement(attempt.Body, inAsyncMethod);
            appendAwaitObserverFrame = previousAppendAwaitObserverFrame;
            var catches = attempt.Catches.Select(clause => clause with
            {
                Body = (BlockStatement)RewriteStatement(clause.Body, inAsyncMethod)
            }).ToArray();
            var finallyBody = attempt.Finally is null ? null : (BlockStatement)RewriteStatement(attempt.Finally, inAsyncMethod);
            return attempt with { Body = body, Catches = catches, Finally = finallyBody };
        }

        Expression? RewriteOptional(Expression? expression, bool inAsyncMethod) =>
            expression is null ? null : RewriteExpression(expression, inAsyncMethod);

        Expression RewriteExpression(Expression expression, bool inAsyncMethod) => expression switch
        {
            AwaitExpression awaited when inAsyncMethod => RewriteAwait(awaited),
            AwaitExpression awaited => ReportAwaitOutsideAsync(awaited),
            AwaitBatchResultExpression result when inAsyncMethod => RewriteAwaitBatchResult(result),
            AwaitBatchResultExpression result => ReportAwaitOutsideAsync(new AwaitExpression(result.Batch.Tasks[result.Index], result.Location)),
            AwaitBatchExpression batch => ReportInvalidAwaitBatch(batch),
            LambdaExpression lambda => RewriteLambda(lambda),
            UnaryExpression unary => unary with { Operand = RewriteExpression(unary.Operand, inAsyncMethod) },
            CastExpression cast => cast with { Operand = RewriteExpression(cast.Operand, inAsyncMethod) },
            BinaryExpression binary => binary with { Left = RewriteExpression(binary.Left, inAsyncMethod), Right = RewriteExpression(binary.Right, inAsyncMethod) },
            ConditionalExpression conditional => conditional with
            {
                Condition = RewriteExpression(conditional.Condition, inAsyncMethod),
                WhenTrue = RewriteExpression(conditional.WhenTrue, inAsyncMethod),
                WhenFalse = RewriteExpression(conditional.WhenFalse, inAsyncMethod)
            },
            MemberAccessExpression member => member with { Receiver = RewriteExpression(member.Receiver, inAsyncMethod) },
            IndexExpression index => index with
            {
                Receiver = RewriteExpression(index.Receiver, inAsyncMethod),
                Indices = index.Indices.Select(item => RewriteExpression(item, inAsyncMethod)).ToArray()
            },
            CallExpression call => call with
            {
                Target = RewriteExpression(call.Target, inAsyncMethod),
                Arguments = call.Arguments.Select(argument => RewriteExpression(argument, inAsyncMethod)).ToArray()
            },
            InlineArrayInitializerExpression initializer => initializer with { Elements = initializer.Elements.Select(item => RewriteExpression(item, inAsyncMethod)).ToArray() },
            AllocationExpression allocation => allocation with
            {
                Dimensions = allocation.Dimensions.Select(item => RewriteExpression(item, inAsyncMethod)).ToArray(),
                Initializer = allocation.Initializer is null ? null : (InlineArrayInitializerExpression)RewriteExpression(allocation.Initializer, inAsyncMethod),
                ConstructorArguments = allocation.ConstructorArguments?.Select(item => RewriteExpression(item, inAsyncMethod)).ToArray()
            },
            NullAssertionExpression assertion => assertion with { Operand = RewriteExpression(assertion.Operand, inAsyncMethod) },
            OverflowContextExpression overflow => overflow with { Value = RewriteExpression(overflow.Value, inAsyncMethod) },
            _ => expression
        };

        Expression RewriteAwait(AwaitExpression awaited)
        {
            if (awaited.Operand is LiteralExpression { Value: not "null" })
                diagnostics.Add("LS0105", DiagnosticSeverity.Error, "The await operator requires a Task or Task<T> operand.", awaited.Location);
            if (awaited.Operand is AllocationExpression allocation && !IsTaskType(allocation.TypeName))
                diagnostics.Add("LS0105", DiagnosticSeverity.Error, "The await operator requires a Task or Task<T> operand.", awaited.Location);

            return new CallExpression(
                new MemberAccessExpression(new NameExpression("System.Threading.Tasks.Task", awaited.Location), "GetAwaitedResult", awaited.Location),
                [
                    RewriteExpression(awaited.Operand, inAsyncMethod: true),
                    new LiteralExpression(QuoteStringLiteral(appendAwaitObserverFrame ? currentAwaitFrame ?? string.Empty : string.Empty), awaited.Location)
                ], awaited.Location, IsAwaitResult: true);
        }

        Expression RewriteAwaitBatchResult(AwaitBatchResultExpression result)
        {
            if (result.Index < 0 || result.Index >= result.Batch.Tasks.Count)
                throw new InvalidOperationException("An await batch result has an invalid task index.");
            if (!awaitBatchGroups.TryGetValue(result.Batch, out var group))
            {
                group = nextAwaitBatchGroup++;
                awaitBatchGroups.Add(result.Batch, group);
            }
            var frame = QuoteStringLiteral(appendAwaitObserverFrame ? currentAwaitFrame ?? string.Empty : string.Empty);
            return new CallExpression(
                new MemberAccessExpression(new NameExpression("System.Threading.Tasks.Task", result.Location), "GetAwaitedResult", result.Location),
                [
                    RewriteExpression(result.Batch.Tasks[result.Index], inAsyncMethod: true),
                    new LiteralExpression(frame, result.Location)
                ], result.Location, IsAwaitResult: true, AwaitBatchGroup: group);
        }

        static bool IsTaskType(string? typeName) => typeName is "Task" or "System.Threading.Tasks.Task" ||
            typeName?.StartsWith("Task<", StringComparison.Ordinal) == true ||
            typeName?.StartsWith("System.Threading.Tasks.Task<", StringComparison.Ordinal) == true;

        Expression RewriteLambda(LambdaExpression lambda)
        {
            if (lambda.ExpressionBody is { } lambdaExpression && ContainsAwait(lambdaExpression) ||
                lambda.BlockBody is not null && ContainsAwait(lambda.BlockBody))
                diagnostics.Add("LS0105", DiagnosticSeverity.Error, "Await inside a lambda requires an async lambda, which is not supported yet.", lambda.Location);
            return lambda;
        }

        Expression ReportAwaitOutsideAsync(AwaitExpression awaited)
        {
            diagnostics.Add("LS0105", DiagnosticSeverity.Error, "The await keyword can only be used inside an async method.", awaited.Location);
            return new UnsupportedExpression("await outside async method", awaited.Location);
        }

        Expression ReportInvalidAwaitBatch(AwaitBatchExpression batch)
        {
            diagnostics.Add("LS0105", DiagnosticSeverity.Error, "A batch await can only be used as the sole right-hand group in a multiple declaration or multiple assignment.", batch.Location);
            return new UnsupportedExpression("invalid batch await context", batch.Location);
        }
    }

    private static bool ContainsAwait(Statement statement) => statement switch
    {
        BlockStatement block => block.Statements.Any(ContainsAwait),
        ReturnStatement returned => returned.Value is not null && ContainsAwait(returned.Value),
        VariableStatement variable => variable.Initializer is not null && ContainsAwait(variable.Initializer),
        VariableDeclarationStatement declaration => declaration.Variables.Any(ContainsAwait),
        MultiAssignmentStatement assignment => assignment.Targets.Any(ContainsAwait) || assignment.Values.Any(ContainsAwait),
        ExpressionStatement expression => ContainsAwait(expression.Value),
        IfStatement conditional => ContainsAwait(conditional.Condition) || ContainsAwait(conditional.Then) || conditional.Else is not null && ContainsAwait(conditional.Else),
        WhileStatement loop => ContainsAwait(loop.Condition) || ContainsAwait(loop.Body),
        DoWhileStatement loop => ContainsAwait(loop.Body) || ContainsAwait(loop.Condition),
        ForStatement loop => loop.Initializer is not null && ContainsAwait(loop.Initializer) || loop.Condition is not null && ContainsAwait(loop.Condition) || loop.Iterators.Any(ContainsAwait) || ContainsAwait(loop.Body),
        ForInStatement loop => ContainsAwait(loop.Collection) || ContainsAwait(loop.Body),
        SwitchStatement selection => ContainsAwait(selection.Value) || selection.Sections.Any(section => section.Labels.Any(label => label.Value is not null && ContainsAwait(label.Value) || label.Guard is not null && ContainsAwait(label.Guard)) || section.Statements.Any(ContainsAwait)),
        TryStatement attempt => ContainsAwait(attempt.Body) || attempt.Catches.Any(clause => ContainsAwait(clause.Body)) || attempt.Finally is not null && ContainsAwait(attempt.Finally),
        ThrowStatement thrown => thrown.Value is not null && ContainsAwait(thrown.Value),
        OverflowContextStatement overflow => ContainsAwait(overflow.Body),
        _ => false
    };

    private static bool ContainsAwait(Expression expression) => expression switch
    {
        AwaitExpression => true,
        AwaitBatchExpression => true,
        AwaitBatchResultExpression => true,
        LambdaExpression lambda => lambda.ExpressionBody is not null && ContainsAwait(lambda.ExpressionBody) || lambda.BlockBody is not null && ContainsAwait(lambda.BlockBody),
        UnaryExpression unary => ContainsAwait(unary.Operand),
        CastExpression cast => ContainsAwait(cast.Operand),
        BinaryExpression binary => ContainsAwait(binary.Left) || ContainsAwait(binary.Right),
        ConditionalExpression conditional => ContainsAwait(conditional.Condition) || ContainsAwait(conditional.WhenTrue) || ContainsAwait(conditional.WhenFalse),
        MemberAccessExpression member => ContainsAwait(member.Receiver),
        IndexExpression index => ContainsAwait(index.Receiver) || index.Indices.Any(ContainsAwait),
        CallExpression call => ContainsAwait(call.Target) || call.Arguments.Any(ContainsAwait),
        InlineArrayInitializerExpression initializer => initializer.Elements.Any(ContainsAwait),
        AllocationExpression allocation => allocation.Dimensions.Any(ContainsAwait) || allocation.Initializer is not null && ContainsAwait(allocation.Initializer) || allocation.ConstructorArguments?.Any(ContainsAwait) == true,
        NullAssertionExpression assertion => ContainsAwait(assertion.Operand),
        OverflowContextExpression overflow => ContainsAwait(overflow.Value),
        _ => false
    };
}
