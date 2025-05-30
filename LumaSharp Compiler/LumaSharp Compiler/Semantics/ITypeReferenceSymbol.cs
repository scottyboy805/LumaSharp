﻿using LumaSharp.Runtime;
using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics
{
    public interface ITypeReferenceSymbol : IReferenceSymbol
    {
        // Properties
        string TypeName { get; }

        string[] NamespaceName { get; }

        PrimitiveType PrimitiveType { get; }

        bool IsPrimitive { get; }
        
        bool IsType { get; }
        
        bool IsContract { get; }

        bool IsEnum { get; }

        _TypeHandle TypeHandle { get; }

        INamespaceReferenceSymbol NamespaceSymbol { get; }

        ITypeReferenceSymbol DeclaringTypeSymbol { get; }

        IGenericParameterIdentifierReferenceSymbol[] GenericParameterSymbols { get; }

        ITypeReferenceSymbol[] BaseTypeSymbols { get; }

        ITypeReferenceSymbol[] TypeMemberSymbols { get; }

        IFieldReferenceSymbol[] FieldMemberSymbols { get; }

        IAccessorReferenceSymbol[] AccessorMemberSymbols { get; }

        IMethodReferenceSymbol[] MethodMemberSymbols { get; }

        IMethodReferenceSymbol[] OperatorMemberSymbols { get; }
    }
}
