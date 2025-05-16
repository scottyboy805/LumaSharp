using LumaSharp.Runtime;
using LumaSharp.Runtime.Reflection;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class MethodModel : MemberModel, IScopeModel, IMethodReferenceSymbol, IScopedReferenceSymbol, IIdentifierReferenceSymbol
    {
        // Private
        private MethodSyntax methodSyntax = null;
        private TypeModel declaringType = null;
        private ITypeReferenceSymbol[] returnTypeSymbols = null;
        private GenericParameterModel[] genericParameterIdentifierSymbols = null;
        private ILocalIdentifierReferenceSymbol[] parameterIdentifierSymbols = null;
        private ILocalIdentifierReferenceSymbol[] localIdentifierSymbols = null;
        private StatementModel[] bodyStatements = null;

        private MetaMethodFlags methodFlags = 0;

        // Properties
        public MethodSyntax MethodSyntax
        {
            get { return methodSyntax; }
        }

        public string MethodName
        {
            get { return methodSyntax.Identifier.Text; }
        }

        public string ScopeName
        {
            get { return "Method Body"; }
        }

        public string IdentifierName
        {
            get { return methodSyntax.Identifier.Text; }
        }

        public bool IsExport
        {
            get { return methodSyntax.HasAccessModifiers == true && methodSyntax.AccessModifiers.Any(m => m.Kind == SyntaxTokenKind.ExportKeyword); }
        }

        public bool IsInternal
        {
            get { return methodSyntax.HasAccessModifiers == true && methodSyntax.AccessModifiers.Any(m => m.Kind == SyntaxTokenKind.InternalKeyword); }
        }

        public bool IsHidden
        {
            get { return methodSyntax.HasAccessModifiers == true && methodSyntax.AccessModifiers.Any(m => m.Kind == SyntaxTokenKind.HiddenKeyword); }
        }

        public bool IsGlobal
        {
            get { return methodSyntax.HasAccessModifiers == true && methodSyntax.AccessModifiers.Any(m => m.Kind == SyntaxTokenKind.GlobalKeyword); }
        }

        public bool IsOverride
        {
            get { return methodSyntax.Override != null; }
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
            get 
            {
                // Check for multiple returns
                if (returnTypeSymbols.Length > 1)
                    throw new InvalidOperationException("Type reference cannot be inferred from method with multiple return types");

                return returnTypeSymbols[0]; 
            }
        }

        public ITypeReferenceSymbol[] ReturnTypeSymbols
        {
            get { return returnTypeSymbols; }
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

        public bool HasReturnTypes
        {
            get 
            { 
                return methodSyntax.ReturnTypes.Count > 1 
                    || methodSyntax.ReturnTypes[0].Identifier.Kind != SyntaxTokenKind.VoidKeyword; 
            }
        }

        public bool HasParameters
        {
            get { return methodSyntax.HasParameters; }
        }

        public bool HasGenericParameters
        {
            get { return methodSyntax.HasGenericParameters; }
        }

        public bool HasOverride
        {
            get { return methodSyntax.Override != null; }
        }

        public bool HasBody
        {
            get { return methodSyntax.HasBody; }
        }

        public MetaMethodFlags MethodFlags
        {
            get { return methodFlags; }
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
            this.methodSyntax = syntax;
            this.declaringType = parent;

            // Create generics
            if (syntax.HasGenericParameters == true)
            {
                // Create symbol array
                genericParameterIdentifierSymbols = new GenericParameterModel[syntax.GenericParameters.Count];

                // Build all
                for (int i = 0; i < genericParameterIdentifierSymbols.Length; i++)
                {
                    // Add to method model
                    genericParameterIdentifierSymbols[i] = new GenericParameterModel(syntax.GenericParameters[i], this);
                }
            }

            // Create body
            if (syntax.HasBody == true)
            {
                this.bodyStatements = syntax.Body.Elements
                    .Select((s, i) => StatementModel.Any(model, this, s, i, this)).ToArray();
            }

            // Create flags
            this.methodFlags = BuildMethodFlags();
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitMethod(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Get symbol token
            memberToken = provider.GetDeclaredSymbolToken(this);

            // Get return type
            returnTypeSymbols = methodSyntax.ReturnTypes.Select(t =>
                provider.ResolveTypeSymbol(this, t)).ToArray();

            // Resolve generics
            if(methodSyntax.HasGenericParameters == true)
            {
                // Resolve all
                for(int i = 0; i < genericParameterIdentifierSymbols.Length; i++)
                {
                    // Resolve symbols
                    genericParameterIdentifierSymbols[i].ResolveSymbols(provider, report);
                }
            }

            // Resolve parameters
            if(methodSyntax.HasParameters == true)
            {
                // Check for global
                int size = (IsGlobal == true) ? methodSyntax.Parameters.Count : methodSyntax.Parameters.Count + 1;

                // Create parameter array
                parameterIdentifierSymbols = new ILocalIdentifierReferenceSymbol[size];

                // Check for global
                if (IsGlobal == false)
                {
                    // Create local model
                    LocalOrParameterModel localModel = new LocalOrParameterModel(
                        Syntax.Parameter(
                            Syntax.TypeReference(declaringType.TypeSyntax),
                            Syntax.Identifier("this")),
                        this, 0);

                    // Create implicit `this` parameter at position 0
                    parameterIdentifierSymbols[0] = localModel;

                    // Resolve symbols
                    localModel.ResolveSymbols(provider, report);
                }

                int offset = IsGlobal ? 0 : 1;

                // Resolve all
                for(int i = 0; i < methodSyntax.Parameters.Count; i++)
                {
                    // Create parameter model
                    LocalOrParameterModel parameterModel = new LocalOrParameterModel(methodSyntax.Parameters[i], this, i + offset);

                    // Store parameter model
                    parameterIdentifierSymbols[i + offset] = parameterModel;

                    // Resolve symbols
                    parameterModel.ResolveSymbols(provider, report);
                }
            }
            else
            {
                // Create empty parameters
                parameterIdentifierSymbols = new ILocalIdentifierReferenceSymbol[0];
            }

            // Check for body
            if(methodSyntax.HasBody == true)
            {
                foreach(StatementModel statement in bodyStatements)
                {
                    statement.ResolveSymbols(provider, report);
                }
            }
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



            ////// ############## TODO - move this to methodbodybuilder
            //// Build arg and locals
            //List<_VariableHandle> argLocals = new List<_VariableHandle>();
            //uint stackOffset = 0;
            //ushort argCount = 0;
            //ushort localCount = 0;
            //uint stackPtrOffset = 0;

            //// Process parameters
            //for(int i = 0; i < parameterIdentifierSymbols.Length; i++)
            //{
            //    // Get type symbol - pass user types by reference via `any` type
            //    ITypeReferenceSymbol parameterTypeSymbol = (parameterIdentifierSymbols[i].TypeSymbol.IsPrimitive == false)
            //        ? provider.ResolveTypeSymbol(PrimitiveType.Any, methodSyntax.Parameters[i].StartToken.Source)
            //        : parameterIdentifierSymbols[i].TypeSymbol;

            //    // Add parameter
            //    argLocals.Add(new _VariableHandle(parameterTypeSymbol.TypeHandle, stackOffset));

            //    // Advance offset
            //    stackOffset += argLocals[argLocals.Count - 1].TypeHandle.TypeSize;

            //    // Increment args
            //    argCount++;
            //}

            //// Get all locals
            //List<ILocalIdentifierReferenceSymbol> locals = new List<ILocalIdentifierReferenceSymbol>();

            //// Add root locals
            //if (localIdentifierSymbols != null)
            //    locals.AddRange(localIdentifierSymbols);

            //// Add scoped locals
            //foreach (IScopedReferenceSymbol scope in DescendantsOfType<IScopedReferenceSymbol>(true))
            //{
            //    // Check for locals
            //    if (scope.LocalsInScope != null)
            //        locals.AddRange(scope.LocalsInScope);
            //}

                
            //// Build all locals
            //for(int i = 0; i < locals.Count; i++)
            //{
            //    // Check for not resolved
            //    if (locals[i].TypeSymbol == null)
            //        continue;

            //    // Add local
            //    argLocals.Add(new _VariableHandle(locals[i].TypeSymbol.TypeHandle, stackOffset)); 

            //    // Advance offset
            //    stackOffset += argLocals[argLocals.Count - 1].TypeHandle.TypeSize;

            //    // Increment locals
            //    localCount++;
            //}

            //// Calculate start exestuation offset
            //stackPtrOffset = stackOffset;


            


            //// Store arg locals
            //argLocalHandles = argLocals.ToArray();
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

        private MetaMethodFlags BuildMethodFlags()
        {
            MetaMethodFlags flags = 0;

            // Check for export
            if(IsExport == true) flags |= MetaMethodFlags.Export;

            // Check for internal
            if (IsInternal == true) flags |= MetaMethodFlags.Internal;

            // Check for hidden
            if (IsHidden == true) flags |= MetaMethodFlags.Hidden;

            // Check for global
            if(IsGlobal == true) flags |= MetaMethodFlags.Global;

            // Check for return value
            if (HasReturnTypes == true) flags |= MetaMethodFlags.ReturnValue;

            // Check for parameters
            if (HasParameters == true) flags |= MetaMethodFlags.ParamValues;

            // Check for initializer

            // Check for abstract
            if (HasBody == false) flags |= MetaMethodFlags.Abstract;

            // Check for override
            if (HasOverride == true) flags |= MetaMethodFlags.Override;

            // Check for generic
            if (HasGenericParameters == true) flags |= MetaMethodFlags.Generic;

            return flags;
        }
    }
}
