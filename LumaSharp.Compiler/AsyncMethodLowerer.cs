namespace LumaSharp.Compiler;

internal static class AsyncMethodLowerer
{
    public static IReadOnlyList<CompilationUnit> Lower(IReadOnlyList<CompilationUnit> units, DiagnosticBag diagnostics)
    {
        var nextGeneratedMethod = 0;
        var nextAwaitBatchGroup = 0;
        var awaitBatchGroups = new Dictionary<AwaitBatchExpression, int>(ReferenceEqualityComparer.Instance);
        string? currentAwaitFrame = null;
        string? currentAsyncTaskName = null;
        HashSet<string> currentAsyncContextShadows = new(StringComparer.Ordinal);
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
                        members.Add(asyncMethod with { ReturnType = GetTaskType(asyncMethod), IsAsync = true });
                    }
                    else if (CanForwardTaskDirectly(type, asyncMethod))
                    {
                        ValidateAsyncMethod(type, asyncMethod);
                        members.Add(asyncMethod with
                        {
                            ReturnType = GetTaskType(asyncMethod),
                            Body = (BlockStatement)RewriteStatement(asyncMethod.Body, inAsyncMethod: false),
                            IsAsync = true
                        });
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
            var taskName = "$async_task_" + nextGeneratedMethod++;
            var asyncContextName = "$async_context_" + nextGeneratedMethod++;
            var sourceFrame = $"at {ownerName}.{method.Name}({parameterTypes})\n";
            currentAwaitFrame = sourceFrame;
            var previousAsyncTaskName = currentAsyncTaskName;
            var previousAsyncContextShadows = currentAsyncContextShadows;
            currentAsyncTaskName = asyncContextName;
            currentAsyncContextShadows = GetAsyncContextShadows(owner, method);
            var asyncBody = (BlockStatement)RewriteStatement(method.Body, inAsyncMethod: true);
            currentAsyncTaskName = previousAsyncTaskName;
            currentAsyncContextShadows = previousAsyncContextShadows;
            currentAwaitFrame = null;
            var bodyMethod = method with
            {
                Name = generatedName,
                Parameters = method.Parameters.Append(new ParameterDeclaration("System.Threading.Task", asyncContextName, location)).ToArray(),
                Body = asyncBody,
                IsAsync = false,
                IsVirtual = false,
                IsOverride = false,
                Accessibility = "private",
                IsCompilerGenerated = true
            };

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
                method.Parameters.Select(parameter => (Expression)new NameExpression(parameter.Name, parameter.Location))
                    .Append(new NameExpression(taskName, location)).ToArray(),
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
                new MemberAccessExpression(new NameExpression("System.Threading.ThreadPool", location), "QueueTask", location),
                [new NameExpression(taskName, location)], location), location);
            var wrapper = method with
            {
                ReturnType = taskType,
                Body = new BlockStatement([taskLocal, setCallback, queueTask, new ReturnStatement(new NameExpression(taskName, location), location)], location),
                IsAsync = true
            };
            return (wrapper, bodyMethod);
        }

        bool CanForwardTaskDirectly(TypeDeclaration owner, MethodDeclaration method)
        {
            if (method.Body is null || ContainsAwait(method.Body))
                return false;

            if (method.Body.Statements.Count != 1 ||
                method.Body.Statements[0] is not ReturnStatement { Value: { } value })
                return false;

            return IsForwardedTaskExpression(owner, method, value);
        }

        bool IsForwardedTaskExpression(TypeDeclaration owner, MethodDeclaration method, Expression expression)
        {
            if (expression is NameExpression name)
                return method.Parameters.Any(parameter =>
                    parameter.Name == name.Name && IsTaskType(parameter.TypeName));
            if (expression is ConditionalExpression conditional)
                return IsForwardedTaskExpression(owner, method, conditional.WhenTrue) &&
                    IsForwardedTaskExpression(owner, method, conditional.WhenFalse);
            if (expression is CastExpression cast)
                return IsForwardedTaskExpression(owner, method, cast.Operand);
            if (expression is NullAssertionExpression assertion)
                return IsForwardedTaskExpression(owner, method, assertion.Operand);
            if (expression is not CallExpression call)
                return false;

            var methodName = call.Target switch
            {
                NameExpression targetName => targetName.Name,
                MemberAccessExpression targetMember => targetMember.Member,
                _ => null
            };
            if (methodName is null)
                return false;

            if (call.Target is NameExpression || call.Target is MemberAccessExpression { Receiver: ThisExpression })
                return HasTaskReturningMethodInHierarchy(owner, methodName);
            if (call.Target is MemberAccessExpression member &&
                GetQualifiedExpressionName(member.Receiver) is { } receiverName)
            {
                var targetType = units.SelectMany(unit => unit.Types).FirstOrDefault(type =>
                    type.Name == receiverName || GetQualifiedTypeName(type) == receiverName ||
                    type.Name == receiverName.Split('.').Last());
                return targetType?.Members.OfType<MethodDeclaration>()
                    .Any(candidate => candidate.Name == methodName &&
                        (candidate.IsAsync || IsTaskType(candidate.ReturnType))) == true;
            }

            return false;
        }

        bool HasTaskReturningMethodInHierarchy(TypeDeclaration type, string name)
        {
            if (type.Members.OfType<MethodDeclaration>().Any(candidate => candidate.Name == name &&
                    (candidate.IsAsync || IsTaskType(candidate.ReturnType))))
                return true;
            foreach (var baseName in type.BaseTypes ?? [])
            {
                var baseType = units.SelectMany(unit => unit.Types).FirstOrDefault(candidate =>
                    candidate.Name == baseName || GetQualifiedTypeName(candidate) == baseName ||
                    candidate.Name == baseName.Split('<')[0].Split('.').Last());
                if (baseType is not null && HasTaskReturningMethodInHierarchy(baseType, name))
                    return true;
            }
            return false;
        }

        static string GetQualifiedTypeName(TypeDeclaration type) => string.IsNullOrEmpty(type.Namespace)
            ? type.Name
            : type.Namespace + "." + type.Name;

        static string? GetQualifiedExpressionName(Expression expression) => expression switch
        {
            NameExpression name => name.Name,
            MemberAccessExpression member => GetQualifiedExpressionName(member.Receiver) is { } prefix
                ? prefix + "." + member.Member
                : member.Member,
            _ => null
        };

        HashSet<string> GetAsyncContextShadows(TypeDeclaration owner, MethodDeclaration method)
        {
            var names = new HashSet<string>(method.Parameters.Select(parameter => parameter.Name), StringComparer.Ordinal);
            AddMemberNames(owner, names, []);
            return names;
        }

        void AddMemberNames(TypeDeclaration type, HashSet<string> names, HashSet<string> visited)
        {
            var qualifiedName = GetQualifiedTypeName(type);
            if (!visited.Add(qualifiedName)) return;
            foreach (var member in type.Members)
            {
                var memberName = member switch
                {
                    FieldDeclaration field => field.Name,
                    ConstantFieldDeclaration constant => constant.Name,
                    MethodDeclaration candidate => candidate.Name,
                    ComputedPropertyDeclaration property => property.Name,
                    _ => null
                };
                if (memberName is not null) names.Add(memberName);
            }

            foreach (var baseName in type.BaseTypes ?? [])
            {
                var baseTypeName = baseName.Split('<')[0];
                var shortName = baseTypeName.Split('.').Last();
                var baseType = units.SelectMany(unit => unit.Types).FirstOrDefault(candidate =>
                    candidate.Name == shortName &&
                    (GetQualifiedTypeName(candidate) == baseTypeName || candidate.Namespace == type.Namespace ||
                        units.SelectMany(unit => unit.Types).Count(item => item.Name == shortName) == 1));
                if (baseType is not null)
                    AddMemberNames(baseType, names, visited);
            }
        }

        void ValidateAsyncMethod(TypeDeclaration owner, MethodDeclaration method)
        {
            var location = method.Location;
            if (method.ReturnsByRef)
                diagnostics.Add("LS0105", DiagnosticSeverity.Error, "An async method cannot return by reference.", location);
            if (method.ReturnType is "Task" or "System.Threading.Task" ||
                method.ReturnType.StartsWith("Task<", StringComparison.Ordinal) ||
                method.ReturnType.StartsWith("System.Threading.Task<", StringComparison.Ordinal))
                diagnostics.Add("LS0105", DiagnosticSeverity.Error, "An async method declares its result type directly; Task is added implicitly.", location);
            if (method.Parameters.Any(parameter => parameter.PassingMode.Length > 0 || parameter.IsScoped))
                diagnostics.Add("LS0105", DiagnosticSeverity.Error, "Async methods cannot have ref, in, out, or scope-bound parameters.", location);
            if (owner.Kind == "struct" && method.IsMutating && !method.IsStatic)
                diagnostics.Add("LS0105", DiagnosticSeverity.Error, "A mutating struct instance method cannot be async because its receiver cannot safely outlive the call.", location);
        }

        static string GetTaskType(MethodDeclaration method) => method.ReturnType == "void"
            ? "System.Threading.Task"
            : $"System.Threading.Task<{method.ReturnType}>";

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

        void AddDeclaredLocalNames(Statement statement, HashSet<string> names)
        {
            switch (statement)
            {
                case VariableStatement variable:
                    names.Add(variable.Name);
                    break;
                case VariableDeclarationStatement declaration:
                    foreach (var variable in declaration.Variables)
                        names.Add(variable.Name);
                    break;
            }
        }

        Statement RewriteScopedStatement(Statement statement, bool inAsyncMethod, IEnumerable<string>? additionalShadows = null)
        {
            var previousShadows = currentAsyncContextShadows;
            currentAsyncContextShadows = new HashSet<string>(previousShadows, StringComparer.Ordinal);
            try
            {
                if (additionalShadows is not null)
                    currentAsyncContextShadows.UnionWith(additionalShadows);
                AddDeclaredLocalNames(statement, currentAsyncContextShadows);
                return RewriteStatement(statement, inAsyncMethod);
            }
            finally
            {
                currentAsyncContextShadows = previousShadows;
            }
        }

        BlockStatement RewriteBlock(BlockStatement block, bool inAsyncMethod)
        {
            var previousShadows = currentAsyncContextShadows;
            currentAsyncContextShadows = new HashSet<string>(previousShadows, StringComparer.Ordinal);
            try
            {
                var statements = new Statement[block.Statements.Count];
                for (var index = 0; index < block.Statements.Count; index++)
                {
                    var child = block.Statements[index];
                    AddDeclaredLocalNames(child, currentAsyncContextShadows);
                    statements[index] = RewriteStatement(child, inAsyncMethod);
                }
                return block with { Statements = statements };
            }
            finally
            {
                currentAsyncContextShadows = previousShadows;
            }
        }

        ForStatement RewriteFor(ForStatement loop, bool inAsyncMethod)
        {
            var previousShadows = currentAsyncContextShadows;
            currentAsyncContextShadows = new HashSet<string>(previousShadows, StringComparer.Ordinal);
            try
            {
                Statement? initializer = null;
                if (loop.Initializer is not null)
                {
                    AddDeclaredLocalNames(loop.Initializer, currentAsyncContextShadows);
                    initializer = RewriteStatement(loop.Initializer, inAsyncMethod);
                }
                return loop with
                {
                    Initializer = initializer,
                    Condition = RewriteOptional(loop.Condition, inAsyncMethod),
                    Iterators = loop.Iterators.Select(expression => RewriteExpression(expression, inAsyncMethod)).ToArray(),
                    Body = RewriteScopedStatement(loop.Body, inAsyncMethod)
                };
            }
            finally
            {
                currentAsyncContextShadows = previousShadows;
            }
        }

        SwitchSection RewriteSwitchSection(SwitchSection section, bool inAsyncMethod)
        {
            var previousShadows = currentAsyncContextShadows;
            currentAsyncContextShadows = new HashSet<string>(previousShadows, StringComparer.Ordinal);
            try
            {
                currentAsyncContextShadows.UnionWith(section.Labels
                    .Where(label => label.PatternVariableName is not null)
                    .Select(label => label.PatternVariableName!));
                var labels = section.Labels.Select(label => label with
                {
                    Value = RewriteOptional(label.Value, inAsyncMethod),
                    Guard = RewriteOptional(label.Guard, inAsyncMethod)
                }).ToArray();
                var captureScope = RewriteBlock(new BlockStatement(section.Statements, section.At), inAsyncMethod);
                return section with
                {
                    Labels = labels,
                    Statements = captureScope.Statements,
                    CaptureScope = captureScope
                };
            }
            finally
            {
                currentAsyncContextShadows = previousShadows;
            }
        }

        static IEnumerable<string> GetSwitchPatternVariableNames(SwitchLabel pattern)
        {
            if (pattern.PatternVariableName is { } name) yield return name;
            if (pattern.LeftPattern is not null)
                foreach (var nested in GetSwitchPatternVariableNames(pattern.LeftPattern)) yield return nested;
            if (pattern.RightPattern is not null)
                foreach (var nested in GetSwitchPatternVariableNames(pattern.RightPattern)) yield return nested;
        }

        SwitchExpression RewriteSwitchExpression(SwitchExpression selection, bool inAsyncMethod)
        {
            var value = RewriteExpression(selection.Value, inAsyncMethod);
            var arms = selection.Arms.Select(arm =>
            {
                var previousShadows = currentAsyncContextShadows;
                currentAsyncContextShadows = new HashSet<string>(previousShadows, StringComparer.Ordinal);
                try
                {
                    currentAsyncContextShadows.UnionWith(GetSwitchPatternVariableNames(arm.Pattern));
                    var pattern = arm.Pattern with
                    {
                        Value = RewriteOptional(arm.Pattern.Value, inAsyncMethod),
                        Guard = RewriteOptional(arm.Pattern.Guard, inAsyncMethod)
                    };
                    var result = RewriteExpression(arm.Result, inAsyncMethod);
                    var captureScope = new BlockStatement([new ExpressionStatement(result, result.Location)], result.Location);
                    return arm with { Pattern = pattern, Result = result, CaptureScope = captureScope };
                }
                finally { currentAsyncContextShadows = previousShadows; }
            }).ToArray();
            return selection with { Value = value, Arms = arms };
        }

        Statement RewriteStatement(Statement statement, bool inAsyncMethod) => statement switch
        {
            BlockStatement block => RewriteBlock(block, inAsyncMethod),
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
                Then = RewriteScopedStatement(conditional.Then, inAsyncMethod, GetPatternVariableNames(conditional.Condition)),
                Else = conditional.Else is null ? null : RewriteScopedStatement(conditional.Else, inAsyncMethod)
            },
            WhileStatement loop => loop with
            {
                Condition = RewriteExpression(loop.Condition, inAsyncMethod),
                Body = RewriteScopedStatement(loop.Body, inAsyncMethod, GetPatternVariableNames(loop.Condition))
            },
            DoWhileStatement loop => loop with { Body = RewriteScopedStatement(loop.Body, inAsyncMethod), Condition = RewriteExpression(loop.Condition, inAsyncMethod) },
            ForStatement loop => RewriteFor(loop, inAsyncMethod),
            ForInStatement loop => loop with
            {
                Collection = RewriteExpression(loop.Collection, inAsyncMethod),
                Body = RewriteScopedStatement(loop.Body, inAsyncMethod, [loop.Name])
            },
            SwitchStatement selection => selection with
            {
                Value = RewriteExpression(selection.Value, inAsyncMethod),
                Sections = selection.Sections.Select(section => RewriteSwitchSection(section, inAsyncMethod)).ToArray()
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
                Body = (BlockStatement)RewriteScopedStatement(clause.Body, inAsyncMethod,
                    clause.VariableName is null ? [] : [clause.VariableName])
            }).ToArray();
            var finallyBody = attempt.Finally is null ? null : (BlockStatement)RewriteScopedStatement(attempt.Finally, inAsyncMethod);
            return attempt with { Body = body, Catches = catches, Finally = finallyBody };
        }

        Expression? RewriteOptional(Expression? expression, bool inAsyncMethod) =>
            expression is null ? null : RewriteExpression(expression, inAsyncMethod);

        static IEnumerable<string> GetPatternVariableNames(Expression expression) => expression switch
        {
            BinaryExpression { Operator: "is", PatternVariableName: { } name } => [name],
            BinaryExpression binary => GetPatternVariableNames(binary.Left).Concat(GetPatternVariableNames(binary.Right)),
            UnaryExpression unary => GetPatternVariableNames(unary.Operand),
            CastExpression cast => GetPatternVariableNames(cast.Operand),
            _ => []
        };

        Expression RewriteBinary(BinaryExpression binary, bool inAsyncMethod)
        {
            var left = RewriteExpression(binary.Left, inAsyncMethod);
            var previousShadows = currentAsyncContextShadows;
            Expression right;
            if (binary.Operator == "&&")
            {
                currentAsyncContextShadows = new HashSet<string>(previousShadows, StringComparer.Ordinal);
                currentAsyncContextShadows.UnionWith(GetPatternVariableNames(binary.Left));
            }
            try
            {
                right = RewriteExpression(binary.Right, inAsyncMethod);
            }
            finally
            {
                currentAsyncContextShadows = previousShadows;
            }
            return binary with { Left = left, Right = right };
        }

        Expression RewriteConditional(ConditionalExpression conditional, bool inAsyncMethod)
        {
            var condition = RewriteExpression(conditional.Condition, inAsyncMethod);
            var previousShadows = currentAsyncContextShadows;
            currentAsyncContextShadows = new HashSet<string>(previousShadows, StringComparer.Ordinal);
            currentAsyncContextShadows.UnionWith(GetPatternVariableNames(conditional.Condition));
            Expression whenTrue;
            try
            {
                whenTrue = RewriteExpression(conditional.WhenTrue, inAsyncMethod);
            }
            finally
            {
                currentAsyncContextShadows = previousShadows;
            }
            return conditional with
            {
                Condition = condition,
                WhenTrue = whenTrue,
                WhenFalse = RewriteExpression(conditional.WhenFalse, inAsyncMethod)
            };
        }

        Expression RewriteExpression(Expression expression, bool inAsyncMethod) => expression switch
        {
            AwaitExpression awaited when inAsyncMethod => RewriteAwait(awaited),
            AwaitExpression awaited => ReportAwaitOutsideAsync(awaited),
            AwaitBatchResultExpression result when inAsyncMethod => RewriteAwaitBatchResult(result),
            AwaitBatchResultExpression result => ReportAwaitOutsideAsync(new AwaitExpression(result.Batch.Tasks[result.Index], result.Location)),
            AwaitBatchExpression batch => ReportInvalidAwaitBatch(batch),
            LambdaExpression lambda => RewriteLambda(lambda, inAsyncMethod),
            MemberAccessExpression { Receiver: NameExpression { Name: "async" } } member
                when inAsyncMethod && member.Member == "IsCancellationRequested" =>
                    new MemberAccessExpression(new NameExpression(currentAsyncTaskName!, member.Location),
                        member.Member, member.Location, IsUnsafeContext: member.IsUnsafeContext),
            MemberAccessExpression { Receiver: NameExpression { Name: "async" } } member =>
                ReportInvalidAsyncContext(member.Location,
                    "The async context supports only IsCancellationRequested and ThrowIfCancellationRequested()."),
            CallExpression { Target: MemberAccessExpression { Receiver: NameExpression { Name: "async" }, Member: "ThrowIfCancellationRequested" } } call
                when inAsyncMethod =>
                    new CallExpression(
                        new MemberAccessExpression(new NameExpression(currentAsyncTaskName!, call.Location),
                            "ThrowIfCancellationRequested", call.Location),
                        call.Arguments.Select(argument => RewriteExpression(argument, inAsyncMethod)).ToArray(),
                        call.Location, call.ArgumentNames, call.ArgumentModes, call.IsUnsafeContext, call.TypeArguments),
            CallExpression { Target: MemberAccessExpression { Receiver: NameExpression { Name: "async" } } } call =>
                ReportInvalidAsyncContext(call.Location,
                    "The async context supports only IsCancellationRequested and ThrowIfCancellationRequested()."),
            CallExpression { Target: NameExpression { Name: "ThrowIfCancellationRequested" } } call
                when inAsyncMethod && !currentAsyncContextShadows.Contains("ThrowIfCancellationRequested") =>
                    new CallExpression(
                        new MemberAccessExpression(new NameExpression(currentAsyncTaskName!, call.Location),
                            "ThrowIfCancellationRequested", call.Location),
                        call.Arguments.Select(argument => RewriteExpression(argument, inAsyncMethod)).ToArray(),
                        call.Location, call.ArgumentNames, call.ArgumentModes, call.IsUnsafeContext, call.TypeArguments),
            NameExpression { Name: "IsCancellationRequested" } name
                when inAsyncMethod && !currentAsyncContextShadows.Contains("IsCancellationRequested") =>
                    new MemberAccessExpression(new NameExpression(currentAsyncTaskName!, name.Location),
                        "IsCancellationRequested", name.Location),
            NameExpression { Name: "async" } name => ReportInvalidAsyncContext(name.Location,
                inAsyncMethod
                    ? "Use async only to qualify IsCancellationRequested or ThrowIfCancellationRequested()."
                    : "The async context is available only inside an async method."),
            UnaryExpression unary => unary with { Operand = RewriteExpression(unary.Operand, inAsyncMethod) },
            CastExpression cast => cast with { Operand = RewriteExpression(cast.Operand, inAsyncMethod) },
            BinaryExpression binary => RewriteBinary(binary, inAsyncMethod),
            ConditionalExpression conditional => RewriteConditional(conditional, inAsyncMethod),
            SwitchExpression selection => RewriteSwitchExpression(selection, inAsyncMethod),
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
                new MemberAccessExpression(new NameExpression("System.Threading.Task", awaited.Location), "GetAwaitedResult", awaited.Location),
                [
                    RewriteExpression(awaited.Operand, inAsyncMethod: true),
                    new LiteralExpression(QuoteStringLiteral(appendAwaitObserverFrame ? currentAwaitFrame ?? string.Empty : string.Empty), awaited.Location),
                    new NameExpression(currentAsyncTaskName!, awaited.Location)
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
                new MemberAccessExpression(new NameExpression("System.Threading.Task", result.Location), "GetAwaitedResult", result.Location),
                [
                    RewriteExpression(result.Batch.Tasks[result.Index], inAsyncMethod: true),
                    new LiteralExpression(frame, result.Location),
                    new NameExpression(currentAsyncTaskName!, result.Location)
                ], result.Location, IsAwaitResult: true, AwaitBatchGroup: group);
        }

        static bool IsTaskType(string? typeName) => typeName is "Task" or "System.Threading.Task" ||
            typeName?.StartsWith("Task<", StringComparison.Ordinal) == true ||
            typeName?.StartsWith("System.Threading.Task<", StringComparison.Ordinal) == true;

        Expression RewriteLambda(LambdaExpression lambda, bool inAsyncMethod)
        {
            if (lambda.ExpressionBody is { } lambdaExpression && ContainsAwait(lambdaExpression) ||
                lambda.BlockBody is not null && ContainsAwait(lambda.BlockBody))
                diagnostics.Add("LS0105", DiagnosticSeverity.Error, "Await inside a lambda requires an async lambda, which is not supported yet.", lambda.Location);
            var addedShadows = lambda.Parameters.Select(parameter => parameter.Name)
                .Where(name => currentAsyncContextShadows.Add(name)).ToArray();
            var rewritten = lambda with
            {
                ExpressionBody = lambda.ExpressionBody is null ? null : RewriteExpression(lambda.ExpressionBody, inAsyncMethod),
                BlockBody = lambda.BlockBody is null ? null : (BlockStatement)RewriteStatement(lambda.BlockBody, inAsyncMethod)
            };
            foreach (var name in addedShadows) currentAsyncContextShadows.Remove(name);
            return rewritten;
        }

        Expression ReportInvalidAsyncContext(SourceLocation location, string message)
        {
            diagnostics.Add("LS0105", DiagnosticSeverity.Error, message, location);
            return new UnsupportedExpression("invalid async context", location);
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
        SwitchExpression selection => ContainsAwait(selection.Value) || selection.Arms.Any(arm =>
            arm.Pattern.Value is not null && ContainsAwait(arm.Pattern.Value) || arm.Pattern.Guard is not null && ContainsAwait(arm.Pattern.Guard) || ContainsAwait(arm.Result)),
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
