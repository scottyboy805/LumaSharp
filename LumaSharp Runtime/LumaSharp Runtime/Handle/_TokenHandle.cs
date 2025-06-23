using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace LumaSharp.Runtime.Handle
{
    public enum TokenKind : byte
    {
        Nil = 0,

        StringReference = 16,

        AssemblyReference = 32,
        PrimitiveTypeReference,
        TypeReference,
        FieldReference,
        AccessorReference,
        MethodReference,

        AssemblyDefinition = 64,
        TypeDefinition,
        FieldDefinition,
        AccessorDefinition,
        MethodDefinition,        

        GenericTypeParameter = 128,
        GenericMethodParameter,
    }

    /// <summary>
    /// 4 byte metadata token.
    /// Represents a table element indexed by token kind and row.
    /// </summary>
    public readonly struct _TokenHandle
    {
        // Public
        public readonly int MetaToken;

        // Properties
        public readonly bool IsNil => Kind == TokenKind.Nil;
        public readonly TokenKind Kind => (TokenKind)((byte)((MetaToken >> 24) & 0xFF));
        public readonly int Row => (MetaToken & 0xFFFFFF);

        // Constructor
        public _TokenHandle(int metaToken)
        {
            this.MetaToken = metaToken;
        }

        public _TokenHandle(TokenKind kind, int row)
        {
            // Check for excessive row
            if (row < 0 || row > 0xFFFFFF)
                throw new ArgumentOutOfRangeException(nameof(row));

            int kindMask = (int)kind << 24;
            int rowMask = (row & 0xFFFFFF);

            // Create token
            this.MetaToken = kindMask | rowMask;
        }

        // Methods
        public override string ToString()
        {
            return $"MetaToken({Kind}, {Row})";
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is _TokenHandle handle 
                && MetaToken == handle.MetaToken;
        }

        public override int GetHashCode()
        {
            return MetaToken.GetHashCode();
        }

        public static _TokenHandle String(int row)
        {
            return new _TokenHandle(TokenKind.StringReference, row);
        }

        public static _TokenHandle FieldRef(int row)
        {
            return new _TokenHandle(TokenKind.FieldReference, row);
        }

        public static _TokenHandle FieldDef(int row)
        {
            return new _TokenHandle(TokenKind.FieldDefinition, row);
        }

        public static _TokenHandle MethodRef(int row)
        {
            return new _TokenHandle(TokenKind.MethodReference, row);
        }

        public static _TokenHandle MethodDef(int row)
        {
            return new _TokenHandle (TokenKind.MethodDefinition, row);
        }

        public static _TokenHandle TypeRef(int row)
        {
            return new _TokenHandle(TokenKind.TypeReference, row);
        }

        public static _TokenHandle TypeDef(int row)
        {
            return new _TokenHandle(TokenKind.TypeDefinition, row);
        }

        public static bool operator==(_TokenHandle a, _TokenHandle b)
        {
            return a.MetaToken == b.MetaToken;
        }

        public static bool operator!=(_TokenHandle a, _TokenHandle b)
        {
            return a.MetaToken != b.MetaToken;
        }

        public static implicit operator int(_TokenHandle token)
        {
            return token.MetaToken;
        }

        public static implicit operator _TokenHandle(int metaToken)
        {
            return new _TokenHandle(metaToken);
        }

        public static implicit operator RuntimeTypeCode(_TokenHandle token)
        {
            return token.Kind == TokenKind.PrimitiveTypeReference
                ? (RuntimeTypeCode)token.Row
                : RuntimeTypeCode.Any;
        }

        public static implicit operator _TokenHandle(RuntimeTypeCode typeCode)
        {
            // Get row
            int row = (int)(0x00FFFFFF & (int)typeCode);

            // Create token
            return new _TokenHandle(TokenKind.PrimitiveTypeReference, row);
        }
    }
}
