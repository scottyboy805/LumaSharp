using LumaSharp.Runtime;
using LumaSharp.Runtime.Handle;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Model.Statement;
using System.Runtime.CompilerServices;

namespace LumaSharp_Compiler.Semantics.Model
{
    public sealed class MethodModel : MemberModel, IScopeModel, IMethodReferenceSymbol, IScopedReferenceSymbol, IIdentifierReferenceSymbol
    {
        // Private
        private MethodSyntax syntax = null;
        private TypeModel declaringType = null;
        private ITypeReferenceSymbol returnTypeSymbol = null;
        private GenericParameterModel[] genericParameterIdentifierSymbols = null;
        private ILocalIdentifierReferenceSymbol[] parameterIdentifierSymbols = null;
        private ILocalIdentifierReferenceSymbol[] localIdentifierSymbols = null;
        private StatementModel[] bodyStatements = null;

        private _MethodHandle methodHandle = default;

        // Properties
        public MethodSyntax Syntax
        {
            get { return syntax; }
        }

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

        public StatementModel[] BodyStatements
        {
            get { return bodyStatements; }
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

        public _MethodHandle MethodHandle
        {
            get { return methodHandle; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                if(HasBody == true)
                {
                    foreach (SymbolModel model in bodyStatements)
                        yield return model;
                }
            }
        }

        // Constructor
        internal MethodModel(SemanticModel model, TypeModel parent, MethodSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.declaringType = parent;

            // Create generics
            if (syntax.HasGenericParameters == true)
            {
                // Create symbol array
                genericParameterIdentifierSymbols = new GenericParameterModel[syntax.GenericParameters.GenericParameterCount];

                // Build all
                for (int i = 0; i < genericParameterIdentifierSymbols.Length; i++)
                {
                    // Add to method model
                    genericParameterIdentifierSymbols[i] = new GenericParameterModel(syntax.GenericParameters.GenericParameters[i], this);
                }
            }

            // Create body
            if (syntax.HasBody == true)
            {
                this.bodyStatements = syntax.Body.Elements
                    .Select((s, i) => StatementModel.Any(model, this, s, i, this)).ToArray();
            }
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitMethod(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Get return type
            returnTypeSymbol = provider.ResolveTypeSymbol(this, syntax.ReturnType);

            // Resolve generics
            if(syntax.HasGenericParameters == true)
            {
                // Resolve all
                for(int i = 0; i < genericParameterIdentifierSymbols.Length; i++)
                {
                    // Resolve symbols
                    genericParameterIdentifierSymbols[i].ResolveSymbols(provider, report);
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
                    localModel.ResolveSymbols(provider, report);
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
                    parameterModel.ResolveSymbols(provider, report);
                }
            }

            //// Check for body
            //if(syntax.HasBody == true)
            //{ 
            //// Resolve locals
            //IEnumerable<VariableDeclarationStatementSyntax> declarations = syntax.Body.DescendantsOfType<VariableDeclarationStatementSyntax>(true);

            //if (declarations.Any() == true)
            //{
            //    // Calculate count
            //    int size = declarations.Count();

            //    // Create array
            //    if (size > 0)
            //    {
            //        // Create symbols array
            //        localIdentifierSymbols = new ILocalIdentifierReferenceSymbol[size];
            //        int index = 0;

            //        // Resolve all
            //        foreach(VariableDeclarationStatementSyntax declaration in declarations)
            //        {
            //            // Create local model
            //            LocalOrParameterModel localModel = new LocalOrParameterModel(declaration, this, index);

            //            // Store local model
            //            localIdentifierSymbols[index] = localModel;
            //            index++;

            //            // Resolve symbols
            //            localModel.ResolveSymbols(provider, report);
            //        }
            //    }
            //}

            // Check for parameters
            if (parameterIdentifierSymbols != null)
            {
                // Check for multiple symbols
                HashSet<string> definedParameters = new HashSet<string>();

                for (int i = 0; i < parameterIdentifierSymbols.Length; i++)
                {
                    // Add the parameter identifier
                    if (definedParameters.Contains(parameterIdentifierSymbols[i].IdentifierName) == false)
                    {
                        definedParameters.Add(parameterIdentifierSymbols[i].IdentifierName);
                    }
                    else
                    {
                        report.ReportMessage(Code.MultipleParameterIdentifiers, MessageSeverity.Error, ((LocalOrParameterModel)parameterIdentifierSymbols[i]).Syntax.StartToken.Source, parameterIdentifierSymbols[i].IdentifierName);
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


                    // Check for any locals
                    if (localIdentifierSymbols != null)
                    {
                        // Check for multiple symbols
                        HashSet<string> definedLocals = new HashSet<string>();

                        for (int i = 0; i < localIdentifierSymbols.Length; i++)
                        {
                            // Add the local identifier
                            if (definedLocals.Contains(localIdentifierSymbols[i].IdentifierName) == false)
                            {
                                definedLocals.Add(localIdentifierSymbols[i].IdentifierName);
                            }
                            else
                            {
                                report.ReportMessage(Code.MultipleLocalIdentifiers, MessageSeverity.Error, ((LocalOrParameterModel)localIdentifierSymbols[i]).Syntax.StartToken.Source, localIdentifierSymbols[i].IdentifierName);
                            }
                        }
                    }
                }
            }


            // Build arg and locals
            List<_StackHandle> argLocals = new List<_StackHandle>();
            uint stackOffset = 0;
            ushort localHandleOffset = 0;
            ushort argPtrOffset = 0;
            ushort localPtrOffset = 0;
            uint stackPtrOffset = 0;

            // Process parameters
            for(int i = 0; i < parameterIdentifierSymbols.Length; i++)
            {
                // Add parameter
                argLocals.Add(new _StackHandle
                {
                    typeHandle = parameterIdentifierSymbols[i].TypeSymbol.TypeHandle,
                    offset = stackOffset,
                });

                // Advance offset
                stackOffset += argLocals[argLocals.Count - 1].typeHandle.size;
            }

            // Update local offset - start of local variables
            localHandleOffset = (ushort)argLocals.Count;
            localPtrOffset = (ushort)stackOffset;

            // Get all locals
            List<ILocalIdentifierReferenceSymbol> locals = new List<ILocalIdentifierReferenceSymbol>();

            // Add root locals
            if (localIdentifierSymbols != null)
                locals.AddRange(localIdentifierSymbols);

            // Add scoped locals
            foreach (IScopedReferenceSymbol scope in DescendantsOfType<IScopedReferenceSymbol>(true))
            {
                // Check for locals
                if (scope.LocalsInScope != null)
                    locals.AddRange(scope.LocalsInScope);
            }

                
            // Build all locals
            for(int i = 0; i < locals.Count; i++)
            {
                // Add local
                argLocals.Add(new _StackHandle
                {
                    typeHandle = locals[i].TypeSymbol.TypeHandle,
                    offset = stackOffset,
                });

                // Advance offset
                stackOffset += argLocals[argLocals.Count - 1].typeHandle.size;
            }

            // Calculate start exestuation offset
            stackPtrOffset = stackOffset;

            // Build method
            this.methodHandle = new _MethodHandle
            {
                localHandleOffset = localHandleOffset,
                argPtrOffset = argPtrOffset,
                localPtrOffset = localPtrOffset,
                stackPtrOffset = stackPtrOffset,
                argLocals = argLocals.ToArray(),
            };
        }

        public override void StaticallyEvaluateMember(ISymbolProvider provider)
        {
            // Check for body
            if(HasBody == true && bodyStatements.Length > 0)
            {
                // Statically evaluate all statements
                foreach(StatementModel statement in bodyStatements)
                {
                    statement.StaticallyEvaluateStatement(provider);
                }
            }
        }

        public VariableModel DeclareScopedLocal(SemanticModel model, VariableDeclarationStatementSyntax syntax, int index)
        {
            // Create the variable
            VariableModel variableModel = new VariableModel(model, this, syntax, index);

            // Register locals
            LocalOrParameterModel[] localModels = variableModel.VariableModels.Where(m  => m != null).ToArray();

            // Declare all
            if(localModels.Length > 0)
            {
                // Make sure array is allocated
                if(localIdentifierSymbols == null)
                    localIdentifierSymbols = Array.Empty<LocalOrParameterModel>();

                int startOffset = localIdentifierSymbols.Length;

                // Resize the array
                Array.Resize(ref localIdentifierSymbols, localIdentifierSymbols.Length + localModels.Length);

                // Append elements
                for(int i = 0; i < localModels.Length; i++)
                {
                    localIdentifierSymbols[startOffset + i] = localModels[i];
                }
            }
            return variableModel;
        }
    }
}
