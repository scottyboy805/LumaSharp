
namespace LumaSharp.Compiler.AST.Visitor
{
    public abstract class SyntaxReplacer : SyntaxRewriter
    {
        // Type
        internal sealed class TokenReplace : SyntaxReplacer
        {
            // Private
            private readonly SyntaxToken match;
            private readonly SyntaxToken replace;

            // Constructor
            public TokenReplace(SyntaxToken match, SyntaxToken replace)
            {
                this.match = match;
                this.replace = replace;
            }

            // Methods
            protected override bool ReplaceSyntax<T>(T syntax, out T replacement)
            {
                replacement = null;
                return false;
            }

            protected override bool ReplaceToken(SyntaxToken token, out SyntaxToken replacement)
            {
                if(token.Equals(match) == true)
                {
                    replacement = replace;
                    return true;
                }
                replacement = default;
                return false;
            }
        }

        internal sealed class SyntaxReplace : SyntaxReplacer
        {
            // Private
            private readonly SyntaxNode match;
            private readonly SyntaxNode replace;

            // Constructor
            public SyntaxReplace(SyntaxNode match, SyntaxNode replace)
            {
                this.match = match;
                this.replace = replace;
            }

            // Methods
            protected override bool ReplaceSyntax<T>(T syntax, out T replacement)
            {
                if (match == syntax)
                {
                    replacement = replace as T;
                    return true;
                }
                replacement = null;
                return false;
            }

            protected override bool ReplaceToken(SyntaxToken token, out SyntaxToken replacement)
            {
                replacement = default;
                return false;
            }
        }

        // Methods
        protected abstract bool ReplaceToken(SyntaxToken token, out SyntaxToken replacement);
        protected abstract bool ReplaceSyntax<T>(T syntax, out T replacement) where T : SyntaxNode;

        private bool Replace(SyntaxToken token, out SyntaxToken replacement)
        {
            // Check for invalid
            if (token.Kind == SyntaxTokenKind.Invalid)
            {
                replacement = default;
                return false;
            }
            return ReplaceToken(token, out replacement);
        }

        private bool Replace<T>(T syntax, out T replacement) where T : SyntaxNode
        {
            // Check for null
            if(syntax == null)
            {
                replacement = default;
                return false;
            }

            // Perform visit
            syntax = DefaultVisit(syntax);

            // Check for null
            if(syntax == null)
            {
                replacement = syntax;
                return true;
            }
            return ReplaceSyntax(syntax, out replacement);
        }

        private bool ReplaceMany(IEnumerable<SyntaxToken> syntax, out IEnumerable<SyntaxToken> replacement)
        {
            // Check for null
            if (syntax == null)
            {
                replacement = null;
                return false;
            }

            bool modified = false;
            List<SyntaxToken> result = new();

            foreach (SyntaxToken s in syntax)
            {
                modified |= Replace(s, out SyntaxToken replaced);
                result.Add(replaced);
            }

            replacement = modified ? result : null;
            return modified;
        }

        private bool ReplaceMany<T>(IEnumerable<T> syntax, out IEnumerable<T> replacement) where T : SyntaxNode
        {
            // Check for null
            if(syntax == null)
            {
                replacement = null;
                return false;
            }

            bool modified = false;
            List<T> result = new();

            foreach(T s in syntax)
            {
                modified |= Replace(s, out T replaced);
                result.Add(replaced);
            }

            replacement = modified ? result : null;
            return modified;
        }

        #region Misc
        public override SyntaxNode VisitArrayParameters(ArrayParametersSyntax arrayParameterList)
        {
            bool modified = Replace(arrayParameterList.LArray, out SyntaxToken lArray);
            modified |= ReplaceMany(arrayParameterList.Separators, out IEnumerable<SyntaxToken> separators);
            modified |= Replace(arrayParameterList.RArray, out SyntaxToken rArray);

            // Check for modified
            if (modified == false)
                return arrayParameterList;

            // Create modified
            return new ArrayParametersSyntax(lArray, separators.ToArray(), rArray);
        }

        public override SyntaxNode VisitAttribute(AttributeSyntax attribute)
        {
            bool modified = Replace(attribute.Hash, out SyntaxToken hash);
            modified |= Replace(attribute.AttributeType, out TypeReferenceSyntax attributeType);
            modified |= Replace(attribute.ArgumentList, out ArgumentListSyntax argumentList);

            // Check for modified
            if(modified == false)
                return attribute;

            // Create modified
            return new AttributeSyntax(hash, attributeType, argumentList);
        }

        //public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax compilationUnit)
        //{
        //    bool modified = ReplaceMany(compilationUnit.GetSpan)
        //}

        public override SyntaxNode VisitConstructorInvoke(ConstructorInvokeSyntax constructorInvoke)
        {
            bool modified = Replace(constructorInvoke.Colon, out SyntaxToken colon);
            modified |= Replace(constructorInvoke.BaseOrThisKeyword, out SyntaxToken baseOrThisKeyword);
            modified |= Replace(constructorInvoke.Arguments, out ArgumentListSyntax argumentList);

            // Check for modified
            if(modified == false)
                return constructorInvoke;

            // Create modified
            return new ConstructorInvokeSyntax(colon, baseOrThisKeyword, argumentList);
        }

        public override SyntaxNode VisitTypeReference(TypeReferenceSyntax typeReference)
        {
            if(typeReference.IsPrimitiveType == true)
            {
                bool modified = Replace(typeReference.Identifier, out SyntaxToken identifier);
                modified |= Replace(typeReference.ArrayParameters, out ArrayParametersSyntax arrayParameters);

                // Check for modified
                if (modified == false)
                    return typeReference;

                // Create modified
                return new TypeReferenceSyntax(identifier, arrayParameters);
            }
            else
            {
                bool modified = Replace(typeReference.Namespace, out SeparatedTokenList ns);
                modified |= ReplaceMany(typeReference.ParentTypes, out IEnumerable<ParentTypeReferenceSyntax> parentTypes);
                modified |= Replace(typeReference.Identifier, out SyntaxToken identifier);
                modified |= Replace(typeReference.GenericArguments, out GenericArgumentListSyntax genericArguments);
                modified |= Replace(typeReference.ArrayParameters, out ArrayParametersSyntax arrayParameters);

                // Check for modified
                if (modified == false)
                    return typeReference;

                // Create modified
                return new TypeReferenceSyntax(ns, parentTypes?.ToArray(), identifier, genericArguments, arrayParameters);
            }
        }
        #endregion

        #region List
        public override SyntaxNode VisitArgumentList(ArgumentListSyntax argumentList)
        {
            bool modified = Replace(argumentList.LParen, out SyntaxToken lParen);
            modified |= Replace(argumentList.RParen, out SyntaxToken rParen);

            SeparatedSyntaxList<ExpressionSyntax> modifiedArguments = VisitSyntaxList(argumentList) as SeparatedSyntaxList<ExpressionSyntax>;
            modified |= modifiedArguments != argumentList;

            // Check for modified
            if (modified == false)
                return argumentList;

            // Create modified
            return new ArgumentListSyntax(lParen, modifiedArguments, rParen);
        }

        public override SyntaxNode VisitBaseTypeList(BaseTypeListSyntax baseTypeList)
        {
            bool modified = Replace(baseTypeList.Colon, out SyntaxToken colon);

            SeparatedSyntaxList<TypeReferenceSyntax> modifiedBaseTypes = VisitSyntaxList(baseTypeList) as SeparatedSyntaxList<TypeReferenceSyntax>;
            modified |= modifiedBaseTypes != baseTypeList;

            // Check for modified
            if (modified == false)
                return baseTypeList;

            // Create modified
            return new BaseTypeListSyntax(colon, modifiedBaseTypes);
        }

        public override SyntaxNode VisitGenericArgumentList(GenericArgumentListSyntax genericArgumentList)
        {
            bool modified = Replace(genericArgumentList.LGeneric, out SyntaxToken lGeneric);
            modified |= Replace(genericArgumentList.RGeneric, out SyntaxToken rGeneric);

            SeparatedSyntaxList<TypeReferenceSyntax> modifiedArguments = VisitSyntaxList(genericArgumentList) as SeparatedSyntaxList<TypeReferenceSyntax>;
            modified |= modifiedArguments != genericArgumentList;

            // Check for modified
            if (modified == false)
                return genericArgumentList;

            // Create modified
            return new GenericArgumentListSyntax(lGeneric, modifiedArguments, rGeneric);
        }

        public override SyntaxNode VisitGenericParameterList(GenericParameterListSyntax genericParameterList)
        {
            bool modified = Replace(genericParameterList.LGeneric, out SyntaxToken lGeneric);
            modified |= Replace(genericParameterList.RGeneric, out SyntaxToken rGeneric);

            SeparatedSyntaxList<GenericParameterSyntax> modifiedParameters = VisitSyntaxList(genericParameterList) as SeparatedSyntaxList<GenericParameterSyntax>;
            modified |= modifiedParameters != genericParameterList;

            // Check for modified
            if (modified == false)
                return genericParameterList;

            // Create modified
            return new GenericParameterListSyntax(lGeneric, modifiedParameters, rGeneric);
        }

        public override SyntaxNode VisitGenericParameter(GenericParameterSyntax genericParameter)
        {
            bool modified = Replace(genericParameter.Identifier, out SyntaxToken identifier);
            modified |= Replace(genericParameter.Colon, out SyntaxToken colon);
            modified |= Replace(genericParameter.ConstraintTypes, out SeparatedSyntaxList<TypeReferenceSyntax> constraintTypes);

            // Check for modified
            if (modified == false)
                return genericParameter;

            // Create modified
            return new GenericParameterSyntax(identifier, colon, constraintTypes);
        }

        public override SyntaxNode VisitParameterList(ParameterListSyntax parameterList)
        {
            bool modified = Replace(parameterList.LParen, out SyntaxToken lParen);
            modified |= Replace(parameterList.RParen, out SyntaxToken rParen);

            SeparatedSyntaxList<ParameterSyntax> modifiedParameters = VisitSyntaxList(parameterList) as SeparatedSyntaxList<ParameterSyntax>;
            modified |= modifiedParameters != parameterList;

            // Check for modified
            if (modified == false)
                return parameterList;

            // Create modified
            return new ParameterListSyntax(lParen, modifiedParameters, rParen);
        }

        public override SyntaxNode VisitParameter(ParameterSyntax parameter)
        {
            SyntaxToken? enumerable = null;

            bool modified = ReplaceMany(parameter.Attributes, out IEnumerable<AttributeSyntax> attributes);
            modified |= Replace(parameter.ParameterType, out TypeReferenceSyntax parameterType);
            modified |= Replace(parameter.Identifier, out SyntaxToken identifier);
            modified |= Replace(parameter.Assignment, out VariableAssignmentExpressionSyntax assignment);
            if(parameter.Enumerable != null)
            {
                modified |= Replace(parameter.Enumerable.Value, out SyntaxToken e);
                enumerable = e;
            }

            // Check for modified
            if (modified == false)
                return parameter;

            // Create modified
            return new ParameterSyntax(attributes.ToArray(), parameterType, identifier, assignment, enumerable);
        }

        public override SyntaxNode VisitSyntaxList<J>(SeparatedSyntaxList<J> list)
        {
            List<SeparatedSyntaxList<J>.SyntaxSeparatedElement> elements = new();
            bool modified = false;

            foreach (var e in list.Elements)
            {
                modified |= Replace(e.Syntax, out J syntax);
                elements.Add(new SeparatedSyntaxList<J>.SyntaxSeparatedElement(
                    syntax, e.Separator));
            }

            // Check for modified
            if (modified == false)
                return list;

            // Create modified
            return new SeparatedSyntaxList<J>(list.SeparatorKind, elements);
        }

        public override SyntaxNode VisitTokenList(SeparatedTokenList list)
        {
            List<SeparatedTokenList.TokenSeparatedElement> elements = new();
            bool modified = false;

            foreach(var e in list.Elements)
            {
                modified |= Replace(e.Token, out SyntaxToken token);
                elements.Add(new SeparatedTokenList.TokenSeparatedElement(
                    token, e.Separator));
            }

            // Check for modified
            if(modified == false)
                return list;

            // Create modified
            return new SeparatedTokenList(list.SeparatorKind, elements, list.TokenKind);
        }
        #endregion

        #region Member
        public override SyntaxNode VisitAccessorBody(AccessorBodySyntax accessorBody)
        {
            bool modified = Replace(accessorBody.Lambda, out SyntaxToken lambda);
            modified |= Replace(accessorBody.Keyword, out SyntaxToken keyword);
            modified |= Replace(accessorBody.Colon, out SyntaxToken colon);
            modified |= Replace(accessorBody.Statement, out StatementSyntax statement);

            // Check for modified
            if (modified == false)
                return accessorBody;

            // Create modified
            return new AccessorBodySyntax(lambda, keyword, colon, statement);
        }

        public override SyntaxNode VisitAccessorLambda(AccessorLambdaSyntax accessorLambda)
        {
            bool modified = Replace(accessorLambda.Lambda, out SyntaxToken lambda);
            modified |= Replace(accessorLambda.Expression, out ExpressionSyntax expression);
            modified |= Replace(accessorLambda.Semicolon, out SyntaxToken semicolon);

            // Check for modified
            if(modified == false)
                return accessorLambda;

            // Create modified
            return new AccessorLambdaSyntax(lambda, expression, semicolon);
        }

        public override SyntaxNode VisitAccessor(AccessorSyntax accessor)
        {
            SyntaxToken? _override = null;

            bool modified = Replace(accessor.Identifier, out SyntaxToken identifier);
            modified |= ReplaceMany(accessor.Attributes, out IEnumerable<AttributeSyntax> attributes);
            modified |= ReplaceMany(accessor.AccessModifiers, out IEnumerable<SyntaxToken> modifiers);
            modified |= Replace(accessor.AccessorType, out TypeReferenceSyntax accessorType);
            if(accessor.Override != null)
            {
                modified |= Replace(accessor.Override.Value, out SyntaxToken o);
                _override = o;
            }
            modified |= ReplaceMany(accessor.AccessorBodies, out IEnumerable<AccessorBodySyntax> bodies);
            modified |= Replace(accessor.Lambda, out AccessorLambdaSyntax lambda);

            // Check for modified
            if (modified == false)
                return accessor;

            // Create modified
            return new AccessorSyntax(identifier, attributes.ToArray(), modifiers.ToArray(), accessorType, _override, bodies.ToArray(), lambda);
        }

        public override SyntaxNode VisitConstructor(ConstructorSyntax constructor)
        {
            bool modified = Replace(constructor.Identifier, out SyntaxToken identifier);
            modified |= ReplaceMany(constructor.Attributes, out IEnumerable<AttributeSyntax> attributes);
            modified |= ReplaceMany(constructor.AccessModifiers, out IEnumerable<SyntaxToken> modifiers);
            modified |= Replace(constructor.Parameters, out ParameterListSyntax parameters);
            modified |= Replace(constructor.ConstructorInvoke, out ConstructorInvokeSyntax constructorInvoke);
            modified |= Replace(constructor.Body, out StatementBlockSyntax body);
            modified |= Replace(constructor.Lambda, out LambdaSyntax lambda);

            // Check for modified
            if(modified == false)
                return constructor;

            // Create modified
            return new ConstructorSyntax(identifier, attributes.ToArray(), modifiers.ToArray(), parameters, constructorInvoke, body, lambda);
        }

        public override SyntaxNode VisitContract(ContractSyntax contract)
        {
            bool modified = Replace(contract.Keyword, out SyntaxToken keyword);
            modified |= Replace(contract.Identifier, out SyntaxToken identifier);
            modified |= ReplaceMany(contract.Attributes, out IEnumerable<AttributeSyntax> attributes);
            modified |= ReplaceMany(contract.AccessModifiers, out IEnumerable<SyntaxToken> modifiers);
            modified |= Replace(contract.GenericParameters, out GenericParameterListSyntax genericParameters);
            modified |= Replace(contract.BaseTypes, out BaseTypeListSyntax baseTypes);
            modified |= Replace(contract.Members, out MemberBlockSyntax members);

            // Check for modified
            if (modified == false)
                return contract;

            // Create modified
            return new ContractSyntax(keyword, identifier, attributes.ToArray(), modifiers.ToArray(), genericParameters, baseTypes, members);
        }

        public override SyntaxNode VisitEnumBlock(EnumBlockSyntax enumBlock)
        {
            bool modified = Replace(enumBlock.LBlock, out SyntaxToken lBlock);
            modified |= Replace(enumBlock.RBlock, out SyntaxToken rBlock);

            SeparatedSyntaxList<EnumFieldSyntax> modifiedFields = VisitSyntaxList(enumBlock) as SeparatedSyntaxList<EnumFieldSyntax>;
            modified |= modifiedFields != enumBlock;

            // Check for modified
            if (modified == false)
                return enumBlock;

            // Create modified
            return new EnumBlockSyntax(lBlock, modifiedFields, rBlock);
        }

        public override SyntaxNode VisitEnumField(EnumFieldSyntax enumField)
        {
            bool modified = Replace(enumField.Identifier, out SyntaxToken identifier);
            modified |= Replace(enumField.FieldAssignment, out VariableAssignmentExpressionSyntax fieldAssignment);

            // Check for modified
            if (modified == false)
                return enumField;

            // Create modified
            return new EnumFieldSyntax(identifier, fieldAssignment);
        }

        public override SyntaxNode VisitEnum(EnumSyntax e)
        {
            bool modified = Replace(e.Keyword, out SyntaxToken keyword);
            modified |= Replace(e.Identifier, out SyntaxToken identifier);
            modified |= ReplaceMany(e.Attributes, out IEnumerable<AttributeSyntax> attributes);
            modified |= ReplaceMany(e.AccessModifiers, out IEnumerable<SyntaxToken> modifiers);
            modified |= Replace(e.UnderlyingType, out BaseTypeListSyntax underlyingType);
            modified |= Replace(e.Body, out EnumBlockSyntax body);

            // Check for modified
            if(modified == false)
                return e;

            // Create modified
            return new EnumSyntax(keyword, identifier, attributes.ToArray(), modifiers.ToArray(), underlyingType, body);
        }

        public override SyntaxNode VisitField(FieldSyntax field)
        {
            bool modified = Replace(field.Identifier, out SyntaxToken identifier);
            modified |= ReplaceMany(field.Attributes, out IEnumerable<AttributeSyntax> attributes);
            modified |= ReplaceMany(field.AccessModifiers, out IEnumerable<SyntaxToken> modifiers);
            modified |= Replace(field.FieldType, out TypeReferenceSyntax typeReference);
            modified |= Replace(field.FieldAssignment, out VariableAssignmentExpressionSyntax fieldAssignment);

            // Check for modified
            if (modified == false)
                return field;

            // Create modified
            return new FieldSyntax(identifier, attributes.ToArray(), modifiers.ToArray(), typeReference, fieldAssignment);
        }

        public override SyntaxNode VisitMemberBlock(MemberBlockSyntax memberBlock)
        {
            bool modified = Replace(memberBlock.LBlock, out SyntaxToken lBlock);
            modified |= ReplaceMany(memberBlock.Members, out IEnumerable<MemberSyntax> members);
            modified |= Replace(memberBlock.RBlock, out SyntaxToken rBlock);

            // Check for modified
            if (modified == false)
                return memberBlock;

            // Create modified
            return new MemberBlockSyntax(lBlock, members, rBlock);
        }

        public override SyntaxNode VisitMethod(MethodSyntax method)
        {
            SyntaxToken? _override = null;

            bool modified = Replace(method.Identifier, out SyntaxToken identifier);
            modified |= ReplaceMany(method.Attributes, out IEnumerable<AttributeSyntax> attributes);
            modified |= ReplaceMany(method.AccessModifiers, out IEnumerable<SyntaxToken> modifiers);
            modified |= Replace(method.ReturnTypes, out SeparatedSyntaxList<TypeReferenceSyntax> returnTypes);
            modified |= Replace(method.GenericParameters, out GenericParameterListSyntax genericParameters);
            modified |= Replace(method.Parameters, out ParameterListSyntax parameters);
            if(method.Override != null)
            {
                modified |= Replace(method.Override.Value, out SyntaxToken o);
                _override = o;
            }
            modified |= Replace(method.Body, out StatementSyntax body);
            modified |= Replace(method.Lambda, out LambdaSyntax lambda);

            // Check for modified
            if (modified == false)
                return method;

            // Create modified
            return new MethodSyntax(identifier, attributes.ToArray(), modifiers.ToArray(), returnTypes, genericParameters, parameters, _override, body, lambda);
        }

        public override SyntaxNode VisitType(TypeSyntax type)
        {
            SyntaxToken? _override = null;

            bool modified = Replace(type.Keyword, out SyntaxToken keyword);
            modified |= Replace(type.Identifier, out SyntaxToken identifier);
            modified |= ReplaceMany(type.Attributes, out IEnumerable<AttributeSyntax> attributes);
            modified |= ReplaceMany(type.AccessModifiers, out IEnumerable<SyntaxToken> modifiers);
            modified |= Replace(type.GenericParameters, out GenericParameterListSyntax genericParameters);
            if (type.Override != null)
            {
                modified |= Replace(type.Override.Value, out SyntaxToken o);
                _override = o;
            }
            modified |= Replace(type.BaseTypes, out BaseTypeListSyntax baseTypes);
            modified |= Replace(type.Members, out MemberBlockSyntax members);

            // Check for modified
            if (modified == false)
                return type;

            // Create modified
            return new TypeSyntax(keyword, identifier, attributes.ToArray(), modifiers.ToArray(), genericParameters, _override, baseTypes, members);
        }
        #endregion

        #region Statement
        public override SyntaxNode VisitAssignStatement(AssignStatementSyntax assignStatement)
        {
            bool modified = Replace(assignStatement.Left, out SeparatedSyntaxList<ExpressionSyntax> left);
            modified |= Replace(assignStatement.AssignExpression, out VariableAssignmentExpressionSyntax assign);
            modified |= Replace(assignStatement.Semicolon, out SyntaxToken semicolon);
            
            // Check for modified
            if(modified == false)
                return assignStatement;

            // Create modified
            return new AssignStatementSyntax(left, assign, semicolon);
        }

        public override SyntaxNode VisitBreakStatement(BreakStatementSyntax breakStatement)
        {
            bool modified = Replace(breakStatement.Keyword, out SyntaxToken keyword);
            modified |= Replace(breakStatement.Semicolon, out SyntaxToken semicolon);

            // Check for modified
            if(modified == false)
                return breakStatement;

            // Create modified
            return new BreakStatementSyntax(keyword, semicolon);
        }

        public override SyntaxNode VisitConditionStatement(ConditionStatementSyntax conditionStatement)
        {
            bool modified = Replace(conditionStatement.Keyword, out SyntaxToken keyword);
            modified |= Replace(conditionStatement.Condition, out ExpressionSyntax condition);
            modified |= Replace(conditionStatement.Alternate, out ConditionStatementSyntax alternate);
            modified |= Replace(conditionStatement.Statement, out StatementSyntax statement);

            // Check for modified
            if(modified == false)
                return conditionStatement;

            // Create modified
            return new ConditionStatementSyntax(keyword, condition, alternate, statement);
        }

        public override SyntaxNode VisitContinueStatement(ContinueStatementSyntax continueStatement)
        {
            bool modified = Replace(continueStatement.Keyword, out SyntaxToken keyword);
            modified |= Replace(continueStatement.Semicolon, out SyntaxToken semicolon);

            // Check for modified
            if(modified == false)
                return continueStatement;

            // Create modified
            return new ContinueStatementSyntax(keyword, semicolon);
        }

        public override SyntaxNode VisitEmptyStatement(EmptyStatementSyntax emptyStatement)
        {
            bool modified = Replace(emptyStatement.Semicolon, out SyntaxToken semicolon);

            // Check for modified
            if (modified == false)
                return emptyStatement;

            // Create modified
            return new EmptyStatementSyntax(semicolon);
        }

        public override SyntaxNode VisitForeachStatement(ForeachStatementSyntax foreachStatement)
        {
            bool modified = Replace(foreachStatement.Keyword, out SyntaxToken keyword);
            modified |= Replace(foreachStatement.TypeReference, out TypeReferenceSyntax typeReference);
            modified |= Replace(foreachStatement.Identifier, out SyntaxToken identifier);
            modified |= Replace(foreachStatement.In, out SyntaxToken _in);
            modified |= Replace(foreachStatement.Expression, out ExpressionSyntax expression);
            modified |= Replace(foreachStatement.Statement, out StatementSyntax statement);

            // Check for modified
            if (modified == false)
                return foreachStatement;

            // Create modified
            return new ForeachStatementSyntax(keyword, typeReference, identifier, _in, expression, statement);
        }

        public override SyntaxNode VisitForStatement(ForStatementSyntax forStatement)
        {
            bool modified = Replace(forStatement.Keyword, out SyntaxToken keyword);
            modified |= Replace(forStatement.Variable, out VariableDeclarationSyntax variable);
            modified |= Replace(forStatement.VariableSemicolon, out SyntaxToken variableSemicolon);
            modified |= Replace(forStatement.Condition, out ExpressionSyntax condition);
            modified |= Replace(forStatement.ConditionSemicolon, out SyntaxToken conditionSemicolon);
            modified |= Replace(forStatement.Increments, out SeparatedSyntaxList<ExpressionSyntax> increments);
            modified |= Replace(forStatement.Statement, out StatementSyntax statement);

            // Check for modified
            if (modified == false)
                return forStatement;

            // Create modified
            return new ForStatementSyntax(keyword, variable, variableSemicolon, condition, conditionSemicolon, increments, statement);
        }

        public override SyntaxNode VisitMethodInvokeStatement(MethodInvokeStatementSyntax methodInvokeStatement)
        {
            bool modified = Replace(methodInvokeStatement.InvokeExpression, out MethodInvokeExpressionSyntax invokeExpression);
            modified |= Replace(methodInvokeStatement.Semicolon, out SyntaxToken semicolon);

            // Check for modified
            if(modified == false)
                return methodInvokeStatement;

            // Create modified
            return new MethodInvokeStatementSyntax(invokeExpression, semicolon);
        }

        public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax returnStatement)
        {
            bool modified = Replace(returnStatement.Keyword, out SyntaxToken keyword);
            modified |= Replace(returnStatement.Expressions, out SeparatedSyntaxList<ExpressionSyntax> expressions);
            modified |= Replace(returnStatement.Semicolon, out SyntaxToken semicolon);

            // Check for modified
            if (modified == false)
                return returnStatement;

            // Create modified
            return new ReturnStatementSyntax(keyword, expressions, semicolon);
        }

        public override SyntaxNode VisitStatementBlock(StatementBlockSyntax statementBlock)
        {
            bool modified = Replace(statementBlock.LBlock, out SyntaxToken lBlock);
            modified |= ReplaceMany(statementBlock.Statements, out IEnumerable<StatementSyntax> statements);
            modified |= Replace(statementBlock.RBlock, out SyntaxToken rBlock);

            // Check for modified
            if(modified == false)
                return statementBlock;

            // Create modified
            return new StatementBlockSyntax(lBlock, statements, rBlock);
        }

        public override SyntaxNode VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax variableDeclarationStatement)
        {
            bool modified = Replace(variableDeclarationStatement.Variable, out VariableDeclarationSyntax variable);
            modified |= Replace(variableDeclarationStatement.Semicolon, out SyntaxToken semicolon);

            // Check for modified
            if(modified == false)
                return variableDeclarationStatement;

            // Create modified
            return new VariableDeclarationStatementSyntax(variable, semicolon);
        }
        #endregion

        #region Expression
        public override SyntaxNode VisitAssignExpression(AssignExpressionSyntax assignExpression)
        {
            bool modified = Replace(assignExpression.Left, out ExpressionSyntax left);
            modified |= Replace(assignExpression.Assign, out SyntaxToken assign);
            modified |= Replace(assignExpression.Right, out ExpressionSyntax right);

            // Check for modified
            if(modified == false)
                return assignExpression;

            // Create modified
            return new AssignExpressionSyntax(left, assign, right);
        }

        public override SyntaxNode VisitBaseExpression(BaseExpressionSyntax baseExpression)
        {
            bool modified = Replace(baseExpression.Keyword, out SyntaxToken keyword);

            // Check for modified
            if (modified == false)
                return baseExpression;

            // Create modified
            return new BaseExpressionSyntax(keyword);
        }

        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax binaryExpression)
        {
            bool modified = Replace(binaryExpression.Left, out ExpressionSyntax left);
            modified |= Replace(binaryExpression.Operation, out SyntaxToken operation);
            modified |= Replace(binaryExpression.Right, out ExpressionSyntax right);

            // Check for modified
            if (modified == false)
                return binaryExpression;

            // Create modified
            return new BinaryExpressionSyntax(left, operation, right);
        }

        public override SyntaxNode VisitCollectionInitializerExpression(CollectionInitializerExpressionSyntax collectionInitializerExpression)
        {
            bool modified = Replace(collectionInitializerExpression.LBlock, out SyntaxToken lBlock);
            modified |= Replace(collectionInitializerExpression.InitializerExpressions, out SeparatedSyntaxList<ExpressionSyntax> initializeExpressions);
            modified |= Replace(collectionInitializerExpression.RBlock, out SyntaxToken rBlock);

            // Check for modified
            if(modified == false)
                return collectionInitializerExpression;

            // Create modified
            return new CollectionInitializerExpressionSyntax(lBlock, initializeExpressions, rBlock);
        }

        public override SyntaxNode VisitIndexExpression(IndexExpressionSyntax indexExpression)
        {
            bool modified = Replace(indexExpression.AccessExpression, out ExpressionSyntax accessExpression);
            modified |= Replace(indexExpression.LArray, out SyntaxToken lArray);
            modified |= Replace(indexExpression.IndexExpressions, out SeparatedSyntaxList<ExpressionSyntax> indexExpressions);
            modified |= Replace(indexExpression.RArray, out SyntaxToken rArray);

            // Check for modified
            if (modified == false)
                return indexExpression;

            // Create modified
            return new IndexExpressionSyntax(accessExpression, lArray, indexExpressions, rArray);
        }

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax literalExpression)
        {
            SyntaxToken? descriptor = null;

            bool modified = Replace(literalExpression.Value, out SyntaxToken value);
            if (literalExpression.Descriptor != null)
            {
                modified |= Replace(literalExpression.Descriptor.Value, out SyntaxToken desc);
                descriptor = desc;
            }

            // Check for modified
            if(modified == false)
                return literalExpression;

            // Create modified
            return new LiteralExpressionSyntax(value, descriptor);
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression)
        {
            bool modified = Replace(memberAccessExpression.AccessExpression, out ExpressionSyntax accessExpression);
            modified |= Replace(memberAccessExpression.Dot, out SyntaxToken dot);
            modified |= Replace(memberAccessExpression.Identifier, out SyntaxToken identifier);

            // Check for modified
            if(modified == false)
                return memberAccessExpression;

            // Create modified
            return new MemberAccessExpressionSyntax(accessExpression, dot, identifier);
        }

        public override SyntaxNode VisitMethodInvokeExpression(MethodInvokeExpressionSyntax methodInvokeExpression)
        {
            bool modified = Replace(methodInvokeExpression.AccessExpression, out ExpressionSyntax accessExpression);
            modified |= Replace(methodInvokeExpression.GenericArgumentList, out GenericArgumentListSyntax genericArguments);
            modified |= Replace(methodInvokeExpression.ArgumentList, out ArgumentListSyntax arguments);

            // Check for modified
            if (modified == false)
                return methodInvokeExpression;

            // Create modified
            return new MethodInvokeExpressionSyntax(accessExpression, genericArguments, arguments);
        }

        public override SyntaxNode VisitNewExpression(NewExpressionSyntax newExpression)
        {
            bool modified = Replace(newExpression.Keyword, out SyntaxToken keyword);
            modified |= Replace(newExpression.NewType, out TypeReferenceSyntax newType);
            modified |= Replace(newExpression.ArgumentList, out ArgumentListSyntax arguments);

            // Check for modified
            if(modified == false)
                return newExpression;

            // Create modified
            return new NewExpressionSyntax(keyword, newType, arguments);
        }

        public override SyntaxNode VisitParenthesizedExpression(ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            bool modified = Replace(parenthesizedExpression.LParen, out SyntaxToken lParen);
            modified |= Replace(parenthesizedExpression.Expression, out ExpressionSyntax expression);
            modified |= Replace(parenthesizedExpression.RParen, out SyntaxToken rParen);

            // Check for modified
            if(modified == false)
                return parenthesizedExpression;

            // Create modified
            return new ParenthesizedExpressionSyntax(lParen, expression, rParen);
        }

        public override SyntaxNode VisitRangeExpression(RangeExpressionSyntax rangeExpression)
        {
            bool modified = Replace(rangeExpression.From, out ExpressionSyntax from);
            modified |= Replace(rangeExpression.Range, out SyntaxToken range);
            modified |= Replace(rangeExpression.To, out ExpressionSyntax to);

            // Check for modified
            if (modified == false)
                return rangeExpression;

            // Create modified
            return new RangeExpressionSyntax(from, range, to);
        }

        public override SyntaxNode VisitSizeofExpression(SizeofExpressionSyntax sizeExpression)
        {
            bool modified = Replace(sizeExpression.Keyword, out SyntaxToken keyword);
            modified |= Replace(sizeExpression.LParen, out SyntaxToken lParen);
            modified |= Replace(sizeExpression.TypeReference, out TypeReferenceSyntax typeReference);
            modified |= Replace(sizeExpression.RParen, out SyntaxToken rParen);

            // Check for modified
            if (modified == false)
                return sizeExpression;

            // Create modified
            return new SizeofExpressionSyntax(keyword, lParen, typeReference, rParen);
        }

        public override SyntaxNode VisitTernaryExpression(TernaryExpressionSyntax ternaryExpression)
        {
            bool modified = Replace(ternaryExpression.Condition, out ExpressionSyntax condition);
            modified |= Replace(ternaryExpression.Ternary, out SyntaxToken ternary);
            modified |= Replace(ternaryExpression.TrueExpression, out ExpressionSyntax trueExpression);
            modified |= Replace(ternaryExpression.Colon, out SyntaxToken colon);
            modified |= Replace(ternaryExpression.FalseExpression, out ExpressionSyntax falseExpression);

            // Check for modified
            if(modified == false)
                return ternaryExpression;

            // Create modified
            return new TernaryExpressionSyntax(condition, ternary, trueExpression, colon, falseExpression);
        }

        public override SyntaxNode VisitThisExpression(ThisExpressionSyntax thisExpression)
        {
            bool modified = Replace(thisExpression.Keyword, out SyntaxToken keyword);

            // Check for modified
            if (modified == false)
                return thisExpression;

            // Create modified
            return new ThisExpressionSyntax(keyword);
        }

        public override SyntaxNode VisitTypeofExpression(TypeofExpressionSyntax typeofExpression)
        {
            bool modified = Replace(typeofExpression.Keyword, out SyntaxToken keyword);
            modified |= Replace(typeofExpression.LParen, out SyntaxToken lParen);
            modified |= Replace(typeofExpression.TypeReference, out TypeReferenceSyntax typeReference);
            modified |= Replace(typeofExpression.RParen, out  SyntaxToken rParen);

            // Check for modified
            if (modified == false)
                return typeofExpression;

            // Create modified
            return new TypeofExpressionSyntax(keyword, lParen, typeReference, rParen);
        }

        public override SyntaxNode VisitUnaryExpression(UnaryExpressionSyntax unaryExpression)
        {
            bool modified = Replace(unaryExpression.Expression, out ExpressionSyntax expression);
            modified |= Replace(unaryExpression.Operation, out SyntaxToken operation);

            // Check for modified
            if(modified == false)
                return unaryExpression;

            // Create modified
            return new UnaryExpressionSyntax(expression, operation, unaryExpression.IsPrefix);
        }

        public override SyntaxNode VisitVariableAssignmentExpression(VariableAssignmentExpressionSyntax variableAssignExpression)
        {
            bool modified = Replace(variableAssignExpression.Assign, out SyntaxToken assign);
            modified |= Replace(variableAssignExpression.AssignExpressions, out SeparatedSyntaxList<ExpressionSyntax> assignExpressions);

            // Check for modified
            if(modified == false)
                return variableAssignExpression;

            // Create modified
            return new VariableAssignmentExpressionSyntax(assign, assignExpressions);
        }

        public override SyntaxNode VisitVariableReferenceExpression(VariableReferenceExpressionSyntax variableReferenceExpression)
        {
            bool modified = Replace(variableReferenceExpression.Identifier, out SyntaxToken identifier);

            // Check for modified
            if (modified == false)
                return variableReferenceExpression;

            // Create modified
            return new VariableReferenceExpressionSyntax(identifier);
        }
        #endregion
    }
}
