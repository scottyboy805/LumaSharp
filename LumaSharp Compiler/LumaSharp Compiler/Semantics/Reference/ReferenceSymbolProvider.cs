using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Reference
{
    internal sealed class ReferenceSymbolProvider : ISymbolProvider
    {
        // Internal
        internal readonly PrimitiveTypeSymbol _void = null;
        internal readonly PrimitiveTypeSymbol _any = null;

        internal readonly PrimitiveTypeSymbol _bool = null;
        internal readonly PrimitiveTypeSymbol _char = null;
        internal readonly PrimitiveTypeSymbol _string = null;
        internal readonly PrimitiveTypeSymbol _i8 = null;
        internal readonly PrimitiveTypeSymbol _u8 = null;
        internal readonly PrimitiveTypeSymbol _i16 = null;
        internal readonly PrimitiveTypeSymbol _u16 = null;
        internal readonly PrimitiveTypeSymbol _i32 = null;
        internal readonly PrimitiveTypeSymbol _u32 = null;
        internal readonly PrimitiveTypeSymbol _i64 = null;
        internal readonly PrimitiveTypeSymbol _u64 = null;
        internal readonly PrimitiveTypeSymbol _float = null;
        internal readonly PrimitiveTypeSymbol _double = null;

        internal readonly EnumTypeSymbol _enum = null;

        // Private
        private ReferenceLibrary runtimeLibrary = null;
        private ReferenceLibrary thisLibrary = null;
        private ReferenceLibrary[] referenceLibraries = null;
        private readonly ReferenceTokenProvider tokenProvider = new();

        private ICompileReportProvider report = null;
        private ReferenceNamespaceResolver namespaceResolver = null;
        private ReferenceTypeResolver typeResolver = null;
        private ReferenceScopedVariableResolver variableResolver = null;

        // Constructor
        public ReferenceSymbolProvider(ReferenceLibrary runtimeLibrary, ReferenceLibrary thisLibrary, ICompileReportProvider report)
        {
            this.runtimeLibrary = runtimeLibrary;
            this.thisLibrary = thisLibrary;
            this.report = report;
            this.namespaceResolver = new ReferenceNamespaceResolver();
            this.typeResolver = new ReferenceTypeResolver(this, report);
            this.variableResolver = new ReferenceScopedVariableResolver(report);

            // Resolve primitive types
            if(runtimeLibrary != null && runtimeLibrary.LibraryName == "runtime")
            {
                // Build primitives
                _void = new PrimitiveTypeSymbol(runtimeLibrary, 0);
                _any = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.Any);

                _bool = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.Bool, _any);
                _char = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.Char, _any);
                _string = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.String, _any);
                _i8 = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.I8, _any);
                _u8 = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.U8, _any);
                _i16 = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.I16, _any);
                _u16 = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.U16, _any);
                _i32 = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.I32, _any);
                _u32 = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.U32, _any);
                _i64 = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.I64, _any);
                _u64 = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.U64, _any);
                _float = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.F32, _any);
                _double = new PrimitiveTypeSymbol(runtimeLibrary, PrimitiveType.F64, _any);

                _enum = new EnumTypeSymbol(runtimeLibrary, _any);
            }
        }

        // Methods
        public _TokenHandle GetSymbolToken(IReferenceSymbol symbol)
        {
            // Try to get the token
            return tokenProvider.GetSymbolToken(symbol);
        }

        public INamespaceReferenceSymbol ResolveNamespaceSymbol(string[] separatedName, SyntaxSpan? span)
        {
            // Check for null or empty
            if (separatedName == null || separatedName.Length == 0)
                return null;

            INamespaceReferenceSymbol resolvedSymbol;

            // Check for resolve namespaces
            if (namespaceResolver.ResolveReferenceNamespaceSymbol(thisLibrary, referenceLibraries, name, out resolvedSymbol) == true)
                return resolvedSymbol;

            // Namespace not found error
            report.ReportDiagnostic(Code.NamespaceNotFound, MessageSeverity.Error, name.StartToken.Span, name.GetSourceText());
            return null;
        }

        public ITypeReferenceSymbol ResolveTypeSymbol(PrimitiveType primitiveType, SyntaxSpan? source)
        {
            ITypeReferenceSymbol primitive = null;

            // Check for void
            if (primitiveType == 0)
            {
                primitive = _void;
            }
            // Check for other primitive type
            else
            {
                primitive = primitiveType switch
                {
                    PrimitiveType.Any => _any,
                    PrimitiveType.Bool => _bool,
                    PrimitiveType.Char => _char,

                    PrimitiveType.I8 => _i8,
                    PrimitiveType.U8 => _u8,
                    PrimitiveType.I16 => _i16,
                    PrimitiveType.U16 => _u16,
                    PrimitiveType.I32 => _i32,
                    PrimitiveType.U32 => _u32,
                    PrimitiveType.I64 => _i64,
                    PrimitiveType.U64 => _u64,

                    PrimitiveType.F32 => _float,
                    PrimitiveType.F64 => _double,

                    _ => throw new NotImplementedException(),
                };
            }

            // Check for null
            if(primitive == null)
            {
                report.ReportDiagnostic(Code.BuiltInTypeNotFound, MessageSeverity.Error, source != null ? source.Value : default, primitiveType.ToString().ToLower());
            }
            return primitive;
        }

        

        public ITypeReferenceSymbol ResolveTypeSymbol(IReferenceSymbol context, string typeName, string[] separatedNamespace, )
        {
            // Check for primitive
            PrimitiveType primitive;
            if (reference.GetPrimitiveType(out primitive) == true)
            {
                // Resolve as primitive
                return ResolveTypeSymbol(primitive, reference.StartToken.Span);
            }

            // Check for void
            if(reference.Identifier.Text == "void" && reference.HasNamespace == false && reference.HasParentType == false)
            {
                // Report errors if void type usage is not correct - used as generic/array for example
                typeResolver.CheckVoidTypeUsage(reference);

                // Check for resolved
                if(_void == null)
                {
                    report.ReportDiagnostic(Code.BuiltInTypeNotFound, MessageSeverity.Error, reference.StartToken.Span, "void");
                }
                return _void;
            }

            // Check for string
            if(reference.Identifier.Text == "string" && reference.HasNamespace == false && reference.HasParentType == false)
            {
                // Check for resolved
                if(_string == null)
                {
                    report.ReportDiagnostic(Code.BuiltInTypeNotFound, MessageSeverity.Error, reference.StartToken.Span, "string");
                }
                return _string;
            }

            // Check for enum
            if(reference.Identifier.Text == "enum" && reference.HasNamespace == false && reference.HasParentType == false)
            {
                // Check for resolved
                if(_enum == null)
                {
                    report.ReportDiagnostic(Code.BuiltInTypeNotFound, MessageSeverity.Error, reference.StartToken.Span, "enum");
                }
                return _enum;
            }

            ITypeReferenceSymbol resolvedSymbol;

            // Check for resolve types in this library first
            if (typeResolver.ResolveReferenceTypeSymbol(thisLibrary, context, reference, out resolvedSymbol) == true)
                return resolvedSymbol;

            // Check for reference libraries secondly

            // Check for simple types reference


            // Type not found error
            report.ReportDiagnostic(Code.TypeNotFound, MessageSeverity.Error, reference.Identifier.Span, reference.Identifier.Text);
            return null;
        }

        public IIdentifierReferenceSymbol ResolveIdentifierSymbol(IReferenceSymbol context, string identifier, SyntaxSpan? span)
        {
            IIdentifierReferenceSymbol resolvedSymbol;

            // Try to resolve identifier
            if (variableResolver.ResolveReferenceIdentifierSymbol(thisLibrary, context, identifier, out resolvedSymbol) == true)
                return resolvedSymbol;

            // Failed to resolve
            report.ReportDiagnostic(Code.IdentifierNotFound, MessageSeverity.Error, span, identifier);
            return null;
        }

        public IIdentifierReferenceSymbol ResolveFieldAccessorIdentifierSymbol(IReferenceSymbol context, string fieldOrAccessorName, SyntaxSpan? span)
        {
            // Check for type
            if (context is ITypeReferenceSymbol typeReference)
            {
                // Get all matches
                int matchCount = 0;
                IIdentifierReferenceSymbol matchSymbol = null;
                
                // Get all fields
                foreach (IFieldReferenceSymbol field in typeReference.FieldMemberSymbols)
                {
                    // Check for matched field name
                    if (field.IdentifierName == fieldOrAccessorName)
                    {
                        matchCount++;
                        matchSymbol = field;
                    }
                }

                // Get all accessors
                foreach (IAccessorReferenceSymbol accessor in typeReference.AccessorMemberSymbols)
                {
                    // Check for matched accessor name
                    if (accessor.IdentifierName == fieldOrAccessorName)
                    {
                        matchCount++;
                        matchSymbol = accessor;
                    }
                }

                // Check for ambiguous match
                if (matchCount > 1)
                    throw new Exception("Ambiguous match: " + typeReference);

                // Check for null
                if (matchSymbol == null)
                    report.ReportDiagnostic(Code.FieldAccessorNotFound, MessageSeverity.Error, span, fieldOrAccessorName, typeReference.TypeName);

                return matchSymbol;
            }

            // 
            throw new InvalidOperationException("Invalid context");
        }

        public IIdentifierReferenceSymbol ResolveMethodIdentifierSymbol(IReferenceSymbol context, string methodName, ITypeReferenceSymbol[] genericArguments, ITypeReferenceSymbol[] argumentTypes, SyntaxSpan? span)
        {
            // Check for type reference
            if (context is ITypeReferenceSymbol typeReference)
            {
                // Get all matches
                List<IMethodReferenceSymbol> matchMethods = new List<IMethodReferenceSymbol>();

                // Get all methods
                foreach(IMethodReferenceSymbol method in typeReference.MethodMemberSymbols)
                {
                    // Check for matched name
                    if(method.IdentifierName == methodName)
                    {
                        matchMethods.Add(method);
                    }
                }

                // Try to resolve best matching method if there are overloads available
                IMethodReferenceSymbol resolveMethod = null;
                int matchIndex = MethodChecker.GetBestMatchingMethodOverload(matchMethods, argumentTypes);

                // Check for matched
                if (matchIndex >= 0)
                {
                    resolveMethod = matchMethods[matchIndex];
                }
                // Check for no inferable match
                else if(matchIndex == -1 && matchMethods.Count > 0)
                {
                    report.ReportDiagnostic(Code.MethodNoMatch, MessageSeverity.Error, span);
                }
                // Check for multiple equally inferable matches
                else if(matchIndex == -2 && matchMethods.Count > 0)
                {
                    report.ReportDiagnostic(Code.MethodAmbiguousMatch, MessageSeverity.Error, span);
                }
                // Check for failure - report as error
                else if (resolveMethod == null)
                {
                    report.ReportDiagnostic(Code.MethodNotFound, MessageSeverity.Error, span, methodName, typeReference.TypeName);
                }

                return resolveMethod;
            }
            
            return null;
        }
    }
}
