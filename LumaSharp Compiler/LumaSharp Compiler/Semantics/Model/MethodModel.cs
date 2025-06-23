using LumaSharp.Runtime.Reflection;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class MethodModel : MemberModel, IMethodReferenceSymbol, IIdentifierReferenceSymbol
    {
        // Private
        private readonly TypeModel declaringType;
        private readonly TypeReferenceModel[] returnTypeModels;
        private readonly GenericParameterModel[] genericParameterModels;
        private readonly LocalOrParameterModel[] parameterModels;
        private readonly ScopeModel scopeModel;

        private MetaMethodFlags methodFlags = 0;
        private _TokenHandle methodToken = default;

        // Properties
        public string MethodName
        {
            get { return MemberName; }
        }

        public string IdentifierName
        {
            get { return MemberName; }
        }

        public bool IsOverride
        {
            get { return (methodFlags & MetaMethodFlags.Override) != 0; }
        }

        public ITypeReferenceSymbol DeclaringTypeSymbol
        {
            get { return declaringType; }
        }

        public ITypeReferenceSymbol TypeSymbol
        {
            get 
            {
                // Check for multiple returns
                if (returnTypeModels.Length > 1)
                    throw new InvalidOperationException("Type reference cannot be inferred from method with multiple return types");

                // Get the evaluated type
                return returnTypeModels[0].EvaluatedTypeSymbol; 
            }
        }

        public ITypeReferenceSymbol[] ReturnTypeSymbols
        {
            get { return returnTypeModels?.Select(r => r.EvaluatedTypeSymbol).ToArray(); }
        }

        public IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols
        {
            get { return genericParameterModels; }
        }

        public ILocalIdentifierReferenceSymbol[] ParameterSymbols
        {
            get { return parameterModels; }
        }

        public bool HasReturnTypes
        {
            get { return returnTypeModels != null; }
        }

        public bool HasParameters
        {
            get { return parameterModels != null; }
        }

        public bool HasGenericParameters
        {
            get { return genericParameterModels != null; }
        }

        public bool HasOverride
        {
            get { return (methodFlags & MetaMethodFlags.Override) != 0; }
        }

        public bool HasBody
        {
            get { return scopeModel != null && scopeModel.HasStatements == true; }
        }

        public MetaMethodFlags MethodFlags
        {
            get { return methodFlags; }
        }

        public override _TokenHandle Token
        {
            get { return methodToken; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                if(HasBody == true)
                {
                    foreach (SymbolModel model in scopeModel.Statements)
                        yield return model;
                }
            }
        }

        // Constructor
        internal MethodModel(MethodSyntax methodSyntax, TypeModel declaringType)
            : base(methodSyntax != null ? methodSyntax.Identifier : default,
                  methodSyntax != null ? methodSyntax.AccessModifiers : null,
                  methodSyntax != null ? methodSyntax.GetSpan() : null)
        {
            // Check for null
            if (methodSyntax == null)
                throw new ArgumentNullException(nameof(methodSyntax));

            this.declaringType = declaringType;
            this.returnTypeModels = methodSyntax.HasReturnType == true
                ? methodSyntax.ReturnTypes.Select(r => new TypeReferenceModel(r)).ToArray()
                : null;
            this.genericParameterModels = methodSyntax.HasGenericParameters == true
                ? methodSyntax.GenericParameters.Select(g => new GenericParameterModel(g)).ToArray()
                : null;
            this.parameterModels = methodSyntax.HasParameters == true
                ? methodSyntax.Parameters.Select((p, i) => new LocalOrParameterModel(p, i)).ToArray()
                : null;

            // Implicit 'this' parameter
            if(IsGlobal == false)
            {
                // Create parameters
                this.parameterModels = InsertImplicitThisParameter(declaringType, this.parameterModels);
            }

            // Set parent
            if(returnTypeModels != null)
            {
                foreach (TypeReferenceModel returnType in returnTypeModels)
                    returnType.parent = this;
            }
            if(genericParameterModels != null)
            {
                foreach(GenericParameterModel genericParameter in genericParameterModels)
                    genericParameter.parent = this;
            }
            if(parameterModels != null)
            {
                foreach(LocalOrParameterModel parameter in parameterModels)
                    parameter.parent = this;
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
            // Resolve return types
            if(returnTypeModels != null)
            {
                foreach (TypeReferenceModel returnType in returnTypeModels)
                    returnType.ResolveSymbols(provider, report);
            }

            // Resolve generic parameters
            if(genericParameterModels != null)
            {
                foreach(GenericParameterModel genericParameter in genericParameterModels)
                    genericParameter.ResolveSymbols(provider, report);
            }

            // Resolve parameters
            if(parameterModels != null)
            {
                foreach(LocalOrParameterModel parameter in parameterModels)
                    parameter.ResolveSymbols(provider, report);
            }

            // Resolve scope
            if(scopeModel != null)
            {
                scopeModel.ResolveSymbols(provider, report);
            }

            // Get token
            this.methodToken = provider.GetSymbolToken(this);




            // Check for parameters
            if (parameterModels != null)
            {
                // Check for multiple symbols
                HashSet<string> definedParameters = new HashSet<string>();

                for (int i = 0; i < parameterModels.Length; i++)
                {
                    // Add the parameter identifier
                    if (definedParameters.Contains(parameterModels[i].IdentifierName) == false)
                    {
                        definedParameters.Add(parameterModels[i].IdentifierName);
                    }
                    else
                    {
                        report.ReportDiagnostic(Code.MultipleParameterIdentifiers, MessageSeverity.Error, parameterModels[i].Span, parameterModels[i].IdentifierName);
                    }
                }
            }
        }

        //public override void StaticallyEvaluateMember(ISymbolProvider provider)
        //{
        //    // Check for body
        //    if(HasBody == true)
        //    {
        //        // Statically evaluate all statements
        //        scopeModel.StaticallyEvaluateStatement(provider);
        //    }
        //}

        private MetaMethodFlags BuildMethodFlags()
        {
            MetaMethodFlags flags = 0;

            // Check for export
            if(HasAccessModifier(AccessModifier.Export) == true) flags |= MetaMethodFlags.Export;

            // Check for internal
            if (HasAccessModifier(AccessModifier.Internal) == true) flags |= MetaMethodFlags.Internal;

            // Check for hidden
            if (HasAccessModifier(AccessModifier.Hidden) == true) flags |= MetaMethodFlags.Hidden;

            // Check for global
            if(HasAccessModifier(AccessModifier.Global) == true) flags |= MetaMethodFlags.Global;

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

        private static LocalOrParameterModel[] InsertImplicitThisParameter(TypeModel declaringType, LocalOrParameterModel[] parameterModels)
        {
            // Check for array
            LocalOrParameterModel[] result = parameterModels != null
                ? new LocalOrParameterModel[parameterModels.Length + 1]
                : new LocalOrParameterModel[1];

            // Insert implicit this
            result[0] = new LocalOrParameterModel("this", declaringType, null, 0, null);

            // Copy remaining
            for(int i = 1; i < result.Length; i++)
            {
                result[i] = parameterModels[i - 1];
            }
            return result;
        }
    }
}
