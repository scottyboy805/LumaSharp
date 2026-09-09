using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.AST.Visitor;
using LumaSharp.Compiler.Semantics.Model;
using System.Reflection;

namespace LumaSharp.Compiler.Semantics
{
    internal sealed class SymbolBuilder : SyntaxVisitor<SymbolBinding>
    {
        // Private
        private SemanticModel model;
        private SymbolBinder binder;

        // Constructor
        public SymbolBuilder(SemanticModel model)
        {
            // Check for null
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            this.model = model;
            this.binder = new SymbolBinder(model);
        }

        public override SymbolBinding VisitAccessor(AccessorSyntax accessor)
        {
            throw new NotImplementedException();
        }

        public override SymbolBinding VisitAccessorBody(AccessorBodySyntax accessorBody)
        {
            throw new NotImplementedException();
        }

        public override SymbolBinding VisitAccessorLambda(AccessorLambdaSyntax accessorLambda)
        {
            throw new NotImplementedException();
        }

        // Methods
        #region NoSymbol
        public override SymbolBinding VisitToken(SyntaxToken token) => default;
        public override SymbolBinding VisitTrivia(SyntaxTrivia trivia) => default;

        public override SymbolBinding VisitArgumentList(ArgumentListSyntax argumentList) => default;
        public override SymbolBinding VisitArrayParameters(ArrayParametersSyntax arrayParameterList) => default;
        public override SymbolBinding VisitAttributeList(AttributeListSyntax attributeList) => default;
        public override SymbolBinding VisitBaseTypeList(BaseTypeListSyntax baseTypeList) => default;
        public override SymbolBinding VisitGenericArgumentList(GenericArgumentListSyntax genericArgumentList) => default;
        public override SymbolBinding VisitGenericParameterList(GenericParameterListSyntax genericParameterList) => default;
        public override SymbolBinding VisitParameterList(ParameterListSyntax parameterList) => default;
        public override SymbolBinding VisitSyntaxList<J>(SeparatedSyntaxList<J> list) => default;
        public override SymbolBinding VisitTokenList(SeparatedTokenList list) => default;

        public override SymbolBinding VisitAssignStatement(AssignStatementSyntax assignStatement) => default;
        public override SymbolBinding VisitBreakStatement(BreakStatementSyntax breakStatement) => default;
        public override SymbolBinding VisitContinueStatement(ContinueStatementSyntax continueStatement) => default;
        public override SymbolBinding VisitEmptyStatement(EmptyStatementSyntax emptyStatement) => default;
        public override SymbolBinding VisitIfStatement(IfStatementSyntax conditionStatement) => default;
        public override SymbolBinding VisitElseClause(ElseClauseSyntax elseStatement) => default;
        public override SymbolBinding VisitForeachStatement(ForeachStatementSyntax foreachStatement) => default;
        public override SymbolBinding VisitForStatement(ForStatementSyntax forStatement) => default;
        public override SymbolBinding VisitMethodInvokeStatement(MethodInvokeStatementSyntax methodInvokeStatement) => default;
        public override SymbolBinding VisitReturnStatement(ReturnStatementSyntax returnStatement) => default;
        public override SymbolBinding VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax variableDeclarationStatement) => default;

        public override SymbolBinding VisitEnumBlock(EnumBlockSyntax enumBlock) => default;
        public override SymbolBinding VisitLambda(LambdaSyntax lambda) => default;
        public override SymbolBinding VisitMemberBlock(MemberBlockSyntax memberBlock) => default;
        public override SymbolBinding VisitStatementBlock(StatementBlockSyntax statementBlock) => default;
        #endregion

        #region Member
        public override SymbolBinding VisitClass(ClassSyntax _class)
        {
            // Create type symbol
            ITypeSymbol type = new TypeModel(model, _class);

            // Create symbol with explicit type
            return new SymbolBinding(type, type);
        }

        public override SymbolBinding VisitStruct(StructSyntax _struct)
        {
            // Create type symbol
            ITypeSymbol type = new TypeModel(model, _struct);

            // Create symbol with explicit type
            return new SymbolBinding(type, type);
        }

        public override SymbolBinding VisitInterface(InterfaceSyntax _interface)
        {
            // Create type symbol
            ITypeSymbol type = new TypeModel(model, _interface);

            // Create symbol with explicit type
            return new SymbolBinding(type, type);
        }

        public override SymbolBinding VisitEnum(EnumSyntax _enum)
        {
            // Create type symbol
            ITypeSymbol type = new TypeModel(model, _enum);

            // Create symbol with explicit type
            return new SymbolBinding(type, type);
        }

        public override SymbolBinding VisitConstructor(ConstructorSyntax constructor)
        {
            // Create from constructor
            return new SymbolBinding(new MethodModel(model, constructor), null);
        }

        public override SymbolBinding VisitMethod(MethodSyntax method)
        {
            // Create method model
            MethodModel methodModel = new MethodModel(model, method);

            // Get return type
            ITypeSymbol returnType = methodModel.GetReturnType();

            // Create from method
            return new SymbolBinding(methodModel, returnType);
        }

        public override SymbolBinding VisitEnumField(EnumFieldSyntax enumField)
        {
            throw new NotImplementedException();
        }

        public override SymbolBinding VisitField(FieldSyntax field)
        {
            // Create field model
            FieldModel fieldModel = new FieldModel(model, field);

            // Get field type
            ITypeSymbol fieldType = fieldModel.GetFieldType();

            // Create binding
            return new SymbolBinding(fieldModel, fieldType);
        }
        #endregion

        public override SymbolBinding VisitAttribute(AttributeSyntax attribute)
        {
            throw new NotImplementedException();
        }

        public override SymbolBinding VisitCompilationUnit(CompilationUnitSyntax compilationUnit)
        {
            throw new NotImplementedException();
        }

        public override SymbolBinding VisitGenericParameter(GenericParameterSyntax genericParameter)
        {
            throw new NotImplementedException();
        }

        public override SymbolBinding VisitGenericParameterConstraints(GenericParameterConstraintsSyntax genericConstraints)
        {
            throw new NotImplementedException();
        }

        public override SymbolBinding VisitNamespace(NamespaceSyntax ns) => default;

        public override SymbolBinding VisitParameter(ParameterSyntax parameter)
        {
            // Create parameter
            ParameterModel parameterModel = new ParameterModel(model, parameter);

            // Get parameter type
            ITypeSymbol parameterType = parameterModel.GetParameterType();

            // Create binding
            return new SymbolBinding(parameterModel, parameterType);
        }

        public override SymbolBinding VisitParentTypeReference(ParentTypeReferenceSyntax parentTypeReference)
        {
            throw new NotImplementedException();
        }

        public override SymbolBinding VisitTypeReference(TypeReferenceSyntax typeReference)
        {
            // Get type symbol
            ITypeSymbol type = binder.ResolveType(typeReference);

            // Create type reference
            return new SymbolBinding(type, type);
        }



        public override SymbolBinding VisitUsing(UsingSyntax import)
        {
            throw new NotImplementedException();
        }

        public override SymbolBinding VisitUsingAlias(UsingAliasSyntax importAlias)
        {
            throw new NotImplementedException();
        }

        public override SymbolBinding VisitVariableDeclaration(VariableDeclarationSyntax variableDeclaration)
        {
            // Get type symbol
            ITypeSymbol variableType = model.GetTypeSymbol(variableDeclaration.VariableType);

            // Check for many
            if (variableDeclaration.Identifiers.Count > 1)
            {
                // Create candidates
                ISymbol[] candidates = new ISymbol[variableDeclaration.Identifiers.Count];

                // Initialize
                for (int i = 0; i < variableDeclaration.Identifiers.Count; i++)
                    candidates[i] = new VariableModel(model, variableDeclaration, i);                

                // Get candidates
                return new SymbolBinding(candidates, variableType);
            }
            else
            {
                // Use single
                return new SymbolBinding(new VariableModel(model, variableDeclaration, 0), variableType);
            }
        }


        #region Expression
        public override SymbolBinding VisitAssignExpression(AssignExpressionSyntax assignExpression) => default;

        public override SymbolBinding VisitBaseExpression(BaseExpressionSyntax baseExpression)
        {
            // Get declaring member
            MemberSyntax declaringMember = baseExpression.ParentOfType<MemberSyntax>();

            // Get declaring class
            ClassSyntax declaringClass = declaringMember switch
            {
                ClassSyntax _class => _class,
                ConstructorSyntax _constructor => _constructor.Parent as ClassSyntax,
                MethodSyntax _method => _method.Parent as ClassSyntax,
                _ => throw new NotSupportedException(declaringMember.GetType().FullName),
            };

            // Check for declaring class
            if(declaringClass != null)
            {
                // Resolve the class symbol
                ITypeSymbol classType = model.GetTypeSymbol(declaringClass);

                // Get base
                ITypeSymbol baseType = classType.GetBaseType();

                // Create the bound expression
                return new SymbolBinding((ISymbol)null, baseType);
            }

            // Could not evaluate base type
            return default;
        }

        public override SymbolBinding VisitBinaryExpression(BinaryExpressionSyntax binaryExpression) => default;  // Should be IOperation instead
        public override SymbolBinding VisitCollectionInitializerExpression(CollectionInitializerExpressionSyntax collectionInitializerExpression) => default;

        public override SymbolBinding VisitConstructorInvoke(ConstructorInvokeSyntax constructorInvoke) => default;
        public override SymbolBinding VisitIndexExpression(IndexExpressionSyntax indexExpression) => default;

        public override SymbolBinding VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression)
        {
            // Resolve the member
            IMemberSymbol member = binder.ResolveMember(memberAccessExpression);

            // Try to get member type
            if(member is IFieldSymbol field)
            {
                // Get field type
                ITypeSymbol fieldType = field.GetFieldType();

                // Create binding
                return new SymbolBinding(member, fieldType);
            }

            // No type binding
            return new SymbolBinding(member, null);
        }

        public override SymbolBinding VisitMethodInvokeExpression(MethodInvokeExpressionSyntax methodInvokeExpression)
        {
            // Resolve the method
            IMethodSymbol method = binder.ResolveMethod(methodInvokeExpression);

            // Check resolved
            if (method == null)
                return default;

            // Resolve type
            ITypeSymbol returnType = method.GetReturnType();

            // Create binding
            return new SymbolBinding(method, returnType);
        }

        public override SymbolBinding VisitNewExpression(NewExpressionSyntax newExpression)
        {
            // Get new type
            ITypeSymbol newType = model.GetTypeSymbol(newExpression.NewType);

            // Create inferred type
            return new SymbolBinding((ISymbol)null, newType);
        }

        public override SymbolBinding VisitParenthesizedExpression(ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            // Get type symbol
            ITypeSymbol expressionType = model.GetTypeSymbol(parenthesizedExpression.Expression);

            // Create inferred type
            return new SymbolBinding((ISymbol)null, expressionType);
        }

        public override SymbolBinding VisitRangeExpression(RangeExpressionSyntax rangeExpression) => default;

        public override SymbolBinding VisitSizeofExpression(SizeofExpressionSyntax sizeExpression)
        {
            // Get sizeof type
            ITypeSymbol evaluatedType = model.GetTypeSymbol(sizeExpression.TypeReference);

            // Get explicit type
            ITypeSymbol sizeType = binder.GetReservedType(ReservedType.Int32);

            // Create binding
            return new SymbolBinding(evaluatedType, sizeType);
        }

        public override SymbolBinding VisitTernaryExpression(TernaryExpressionSyntax ternaryExpression) => default;

        public override SymbolBinding VisitThisExpression(ThisExpressionSyntax thisExpression)
        {
            // Get declaring member
            MemberSyntax declaringMember = thisExpression.ParentOfType<MemberSyntax>();

            // Get declaring class
            MemberSyntax declaringType = declaringMember switch
            {
                ClassSyntax _class => _class,
                StructSyntax _struct => _struct,
                ConstructorSyntax _constructor => _constructor.Parent as MemberSyntax,
                MethodSyntax _method => _method.Parent as MemberSyntax,
                _ => throw new NotSupportedException(declaringMember.GetType().FullName),
            };

            // Check for declaring class
            if (declaringMember != null)
            {
                // Resolve the class symbol
                ITypeSymbol classType = model.GetTypeSymbol(declaringMember);

                // Create the bound expression
                return new SymbolBinding((ISymbol)null, classType);
            }

            // Could not evaluate base type
            return default;
        }

        public override SymbolBinding VisitTypeofExpression(TypeofExpressionSyntax typeofExpression) => default;
        public override SymbolBinding VisitUnaryExpression(UnaryExpressionSyntax unaryExpression) => default;
        public override SymbolBinding VisitVariableAssignmentExpression(VariableAssignmentExpressionSyntax variableAssignExpression) => default;

        public override SymbolBinding VisitVariableReferenceExpression(VariableReferenceExpressionSyntax variableReferenceExpression)
        {
            // Resolve the identifier
            ISymbol identifierSymbol = binder.ResolveIdentifier(variableReferenceExpression);

            // Check for parameter
            if(identifierSymbol is IParameterSymbol parameter)
            {
                // Get the parameter type
                ITypeSymbol parameterType = parameter.GetParameterType();

                // Create binding
                return new SymbolBinding(identifierSymbol, parameterType);
            }

            // Check for local
            if(identifierSymbol is IVariableSymbol variable)
            {
                // Get the variable type
                ITypeSymbol variableType = variable.GetVariableType();

                // Create binding
                return new SymbolBinding(identifierSymbol, variableType);
            }

            // Check for field
            if(identifierSymbol is IFieldSymbol field)
            {
                // Get the field type
                ITypeSymbol fieldType = field.GetFieldType();

                // Create binding
                return new SymbolBinding(identifierSymbol, fieldType);
            }
            throw new Exception("Unresolved variable reference: " + variableReferenceExpression.Identifier.Text);
        }

        public override SymbolBinding VisitLiteralExpression(LiteralExpressionSyntax literalExpression)
        {
            // Create constant model
            ConstantModel constant = new ConstantModel(model, literalExpression);

            // Get inferred type
            ITypeSymbol inferredType = constant.ConstantKind switch
            {
                // Boolean
                ConstantKind.Boolean => binder.GetReservedType(ReservedType.Bool),

                // Integer
                ConstantKind.Integer => GetInferredLiteralIntegerType(constant.IntegerValue.Value),

                // Decimal
                ConstantKind.Decimal => GetInferredLiteralDecimalType(constant.DecimalValue.Value),

                // String
                ConstantKind.String => binder.GetReservedType(ReservedType.String),

                // Default
                _ => throw new NotSupportedException(constant.ConstantKind.ToString()),
            };

            // Create constant
            return new SymbolBinding(constant, inferredType, constant);
        }

        private ITypeSymbol GetInferredLiteralIntegerType(ulong value)
        {
            // Check for int8
            if (value <= byte.MaxValue)
                return binder.GetReservedType(ReservedType.UInt8);

            // Check for int16
            if (value <= ushort.MaxValue)
                return binder.GetReservedType(ReservedType.UInt16);

            // Check for int32
            if (value <= uint.MaxValue)
                return binder.GetReservedType(ReservedType.UInt32);

            // Must be int64
            return binder.GetReservedType(ReservedType.UInt64);
        }

        private ITypeSymbol GetInferredLiteralDecimalType(double value)
        {
            // Check for float
            if (value >= float.MinValue && value <= float.MaxValue)
                return binder.GetReservedType(ReservedType.Float);

            // Must be double
            return binder.GetReservedType(ReservedType.Double);
        }
        #endregion
    }
}
