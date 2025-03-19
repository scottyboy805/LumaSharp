using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class LocalOrParameterModel : ILocalIdentifierReferenceSymbol
    {
        // Private
        private SyntaxNode syntax = null;
        private IReferenceSymbol parent = null;
        private ITypeReferenceSymbol type = null;
        private string identifier = null;
        private int index = 0;
        private int statementIndex = 0;
        private bool isByReference = false;

        // Properties
        public SyntaxNode Syntax
        {
            get { return syntax; }
        }

        public bool IsLocal
        {
            get { return syntax is VariableDeclarationStatementSyntax; }
        }

        public bool IsParameter
        {
            get { return syntax is ParameterSyntax; }
        }

        public bool IsByReference
        {
            get { return isByReference; }
        }

        public bool IsOptional
        {
            get { return false; }
        }

        public int Index
        {
            get { return index; }
        }

        public int DeclareIndex
        {
            get { return statementIndex; }
        }

        public IReferenceSymbol ParentSymbol
        {
            get { return parent; }
        }

        public ITypeReferenceSymbol TypeSymbol
        {
            get { return type; }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return type.LibrarySymbol; }
        }

        public _TokenHandle SymbolToken
        {
            get { return default; }
        }

        public string IdentifierName
        {
            get { return identifier; }
        }

        // Constructor
        public LocalOrParameterModel(VariableDeclarationStatementSyntax syntax, IReferenceSymbol parent, int localIndex, int variableIndex, int statementIndex)
        {
            this.syntax = syntax;
            this.parent = parent;
            this.identifier = syntax.Identifiers[variableIndex].Text;
            this.index = localIndex;
            this.statementIndex = statementIndex;
        }

        public LocalOrParameterModel(ParameterSyntax syntax, IReferenceSymbol parent, int paramIndex)
        {
            this.syntax = syntax;
            this.parent = parent;
            this.identifier = syntax.Identifier.Text;
            this.index = paramIndex;
            this.isByReference = syntax.IsByReference;
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
