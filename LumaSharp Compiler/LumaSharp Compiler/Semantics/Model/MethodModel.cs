using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model.Statement;
using System.Runtime.CompilerServices;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class MethodModel : MemberModel, IMethodReferenceSymbol, IScopedReferenceSymbol, IIdentifierReferenceSymbol
    {
        // Private
        private MethodSyntax syntax = null;
        private TypeModel declaringType = null;
        private ITypeReferenceSymbol returnTypeSymbol = null;
        private IGenericParameterIdentifierReferenceSymbol[] genericParameterIdentifierSymbols = null;
        private ILocalIdentifierReferenceSymbol[] parameterIdentifierSymbols = null;
        private ILocalIdentifierReferenceSymbol[] localIdentifierSymbols = null;
        private StatementModel[] bodyStatements = null;

        // Properties
        public string MethodName
        {
            get { return syntax.Identifier.Text; }
        }

        public string ScopeName
        {
            get { return "Method Body"; }
        }

        public string IdentifierName
        {
            get { return syntax.Identifier.Text; }
        }

        public bool IsGlobal
        {
            get { return syntax.HasAccessModifiers == true && syntax.AccessModifiers.FirstOrDefault(m => m.Text == "global") != null; }
        }

        public ITypeReferenceSymbol DeclaringTypeSymbol
        {
            get { return declaringType; }
        }

        public IReferenceSymbol ParentSymbol
        {
            get { return declaringType; }
        }

        public ITypeReferenceSymbol TypeSymbol
        {
            get { return returnTypeSymbol; }
        }

        public ITypeReferenceSymbol ReturnTypeSymbol
        {
            get { return returnTypeSymbol; }
        }

        public IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols
        {
            get { return genericParameterIdentifierSymbols; }
        }

        public ILocalIdentifierReferenceSymbol[] ParameterSymbols
        {
            get { return parameterIdentifierSymbols; }
        }

        public ILocalIdentifierReferenceSymbol[] LocalsInScope
        {
            get { return localIdentifierSymbols; }
        }

        public bool HasReturnType
        {
            get { return syntax.ReturnType.Identifier.Text != "void"; }
        }

        public bool HasGenericParameters
        {
            get { return syntax.HasGenericParameters; }
        }

        public bool HasBody
        {
            get { return syntax.HasBody; }
        }

        // Constructor
        internal MethodModel(SemanticModel model, TypeModel parent, MethodSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.declaringType = parent;
            
            // Create body
            if (syntax.HasBody == true)
            {
                this.bodyStatements = syntax.Body.Elements
                    .Select(s => StatementModel.Any(model, this, s)).ToArray();
            }
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Get return type
            returnTypeSymbol = provider.ResolveTypeSymbol(declaringType, syntax.ReturnType);

            // Resolve generics
            if(syntax.HasGenericParameters == true)
            {
                // Create symbol array
                genericParameterIdentifierSymbols = new IGenericParameterIdentifierReferenceSymbol[syntax.GenericParameters.GenericParameterCount];

                // Resolve all
                for(int i = 0; i < genericParameterIdentifierSymbols.Length; i++)
                {
                    // Create generic
                    GenericParameterModel genericModel = new GenericParameterModel(syntax.GenericParameters.GenericParameters[i], this);

                    // Add to method model
                    genericParameterIdentifierSymbols[i] = genericModel;

                    // Resolve symbols
                    genericModel.ResolveSymbols(provider);
                }
            }

            // Resolve parameters
            if(syntax.HasParameters == true)
            {
                // Check for global
                int size = (IsGlobal == true) ? syntax.Parameters.ParameterCount : syntax.Parameters.ParameterCount + 1;

                // Create parameter array
                parameterIdentifierSymbols = new ILocalIdentifierReferenceSymbol[size];

                // Check for global
                if (IsGlobal == false)
                {
                    // Create local model
                    LocalOrParameterModel localModel = new LocalOrParameterModel(
                        new ParameterSyntax(new TypeReferenceSyntax(declaringType.Syntax), "this"), this, 0);

                    // Create implicit `this` parameter at position 0
                    parameterIdentifierSymbols[0] = localModel;

                    // Resolve symbols
                    localModel.ResolveSymbols(provider);
                }

                int offset = IsGlobal ? 0 : 1;

                // Resolve all
                for(int i = 0; i < syntax.Parameters.ParameterCount; i++)
                {
                    // Create parameter model
                    LocalOrParameterModel parameterModel = new LocalOrParameterModel(syntax.Parameters.Parameters[i], this, i + offset);

                    // Store parameter model
                    parameterIdentifierSymbols[i + offset] = parameterModel;

                    // Resolve symbols
                    parameterModel.ResolveSymbols(provider);
                }
            }

            // Resolve locals
            IEnumerable<VariableDeclarationStatementSyntax> declarations = syntax.Body.DescendantsOfType<VariableDeclarationStatementSyntax>(true);

            if (declarations.Any() == true)
            {
                // Calculate count
                int size = declarations.Count();

                // Create array
                if (size > 0)
                {
                    // Create symbols array
                    localIdentifierSymbols = new ILocalIdentifierReferenceSymbol[size];
                    int index = 0;

                    // Resolve all
                    foreach(VariableDeclarationStatementSyntax declaration in declarations)
                    {
                        // Create local model
                        LocalOrParameterModel localModel = new LocalOrParameterModel(declaration, this, index);

                        // Store local model
                        localIdentifierSymbols[index] = localModel;
                        index++;

                        // Resolve symbols
                        localModel.ResolveSymbols(provider);
                    }
                }
            }


            // Check for body provided
            if (HasBody == true)
            {
                // Resolve all body statements
                foreach (StatementModel statement in bodyStatements)
                {
                    // Resolve the statement
                    statement.ResolveSymbols(provider, report);
                }
            }
        }
    }
}
