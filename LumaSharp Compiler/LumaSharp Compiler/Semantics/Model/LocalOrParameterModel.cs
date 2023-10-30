using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class LocalOrParameterModel : ILocalIdentifierReferenceSymbol
    {
        // Private
        private SyntaxNode syntax = null;
        private IReferenceSymbol parent = null;
        private ITypeReferenceSymbol type = null;
        private string identifier = null;
        private int index = 0;

        // Properties
        public bool IsLocal
        {
            get { return syntax is VariableDeclarationStatementSyntax; }
        }

        public bool IsParameter
        {
            get { return syntax is ParameterSyntax; }
        }

        public bool IsOptional
        {
            get { return false; }
        }

        public int Index
        {
            get { return index; }
        }

        public IReferenceSymbol ParentSymbol
        {
            get { return parent; }
        }

        public ITypeReferenceSymbol TypeSymbol
        {
            get { return type; }
        }

        public string IdentifierName
        {
            get { return identifier; }
        }

        // Constructor
        public LocalOrParameterModel(VariableDeclarationStatementSyntax syntax, IReferenceSymbol parent, int localIndex, int variableIndex = 0)
        {
            this.syntax = syntax;
            this.parent = parent;
            this.identifier = syntax.Identifiers[variableIndex].Text;
            this.index = localIndex;
        }

        public LocalOrParameterModel(ParameterSyntax syntax, IReferenceSymbol parent, int paramIndex)
        {
            this.syntax = syntax;
            this.parent = parent;
            this.identifier = syntax.Identifier.Text;
            this.index = paramIndex;
        }

        // Methods
        public void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Check for variable
            if(syntax is VariableDeclarationStatementSyntax)
            {
                // Resolve variable type
                type = provider.ResolveTypeSymbol(parent, ((VariableDeclarationStatementSyntax)syntax).VariableType);
            }
            // Check for parameter
            else if(syntax is ParameterSyntax)
            {
                // Resolve parameter type
                type = provider.ResolveTypeSymbol(parent, ((ParameterSyntax)syntax).ParameterType);
            }
        }
    }
}
