using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics.Model;

namespace LumaSharp.Compiler.Semantics
{
    internal sealed class SymbolBinder
    {
        // Private
        private SemanticModel model;
        private Dictionary<ReservedType, ITypeSymbol> reservedTypes = new();

        // Constructor
        public SymbolBinder(SemanticModel model)
        {
            this.model = model;

            // Set reserved types
            InitializeReservedTypes();
        }

        // Methods
        public ITypeSymbol GetReservedType(ReservedType type)
        {
            // Get the reserve symbol
            return reservedTypes[type];
        }

        public ITypeSymbol ResolveType(TypeReferenceSyntax typeReference)
        {
            // Check for built in
            if(typeReference.IsReservedType == true)
            {
                // Get reserved type
                typeReference.GetReservedType(out ReservedType reservedType);

                // Create reserved type
                return reservedTypes[reservedType];
            }

            throw new NotImplementedException();
        }

        public IMethodSymbol ResolveConstructor(NewExpressionSyntax newInvoke)
        {
            throw new NotImplementedException();
        }

        public IMethodSymbol ResolveMethod(MethodInvokeExpressionSyntax methodInvoke)
        {
            throw new NotImplementedException();
        }

        public IMemberSymbol ResolveMember(MemberAccessExpressionSyntax memberAccess)
        {
            throw new NotImplementedException();
        }

        public ISymbol ResolveIdentifier(ExpressionSyntax expression)
        {
            // Get parent method or constructor
            MemberSyntax parentMember = expression.ParentOfType<MemberSyntax>();

            switch(expression)
            {
                case VariableReferenceExpressionSyntax variableReferenceExpression:
                    {
                        /// ### Resolve to parameter
                        // Get parameters from parent method
                        ParameterListSyntax parameters = parentMember switch
                        {
                            MethodSyntax method => method.Parameters,
                            ConstructorSyntax constructor => constructor.Parameters,

                            _ => throw new InvalidOperationException("Unexpected member: " + parentMember),
                        };

                        // Check for any parameters
                        if (parameters != null && parameters.Count > 0)
                        {
                            for (int i = 0; i < parameters.Count; i++)
                            {
                                // Get parameter symbol
                                IParameterSymbol parameterSymbol = model.GetSymbol(parameters[i]).Symbol as IParameterSymbol;

                                // Check for parameter name match
                                if (parameterSymbol != null && parameterSymbol.Name == variableReferenceExpression.Identifier.Text)
                                    return parameterSymbol;
                            }
                        }


                        /// ### Resolve to local variable
                        // Get all variable declarations from member
                        StatementSyntax statementBody = parentMember switch
                        {
                            MethodSyntax method => method.Body,
                            ConstructorSyntax constructor => constructor.Body,

                            _ => throw new InvalidOperationException("Unexpected member: " + parentMember),
                        };

                        // Check for statement
                        if(statementBody != null)
                        {
                            // Process all statemements
                            foreach(StatementSyntax statement in statementBody is StatementBlockSyntax block
                                ? block.Statements : Enumerable.Repeat(statementBody, 1))
                            {
                                // Check for variable
                                if(statement is VariableDeclarationStatementSyntax variableStatement)
                                {
                                    // Get variable symbols
                                    SymbolInfo symbolInfo = model.GetSymbol(variableStatement.Variable);

                                    // Process single
                                    if(symbolInfo.Symbol != null)
                                    {
                                        // Check for variable symbol
                                        if (symbolInfo.Symbol is IVariableSymbol variableSymbol && variableSymbol.Name == variableReferenceExpression.Identifier.Text)
                                            return variableSymbol;
                                    }
                                    // Process all candidates
                                    else if (symbolInfo.CandidateSymbols != null)
                                    {
                                        foreach (ISymbol symbol in symbolInfo.CandidateSymbols)
                                        {
                                            // Check for variable symbol
                                            if (symbol is IVariableSymbol variableSymbol && variableSymbol.Name == variableReferenceExpression.Identifier.Text)
                                                return variableSymbol;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
            }

            // No matching symbol
            return null;
        }

        private void InitializeReservedTypes()
        {
            // Get all reserved types
            foreach(ReservedType reservedType in Enum.GetValues<ReservedType>())
            {
                // Create reserved type
                reservedTypes[reservedType] = new TypeModel(model, Syntax.Struct(reservedType.ToString()), reservedType);
            }
        }
    }
}
