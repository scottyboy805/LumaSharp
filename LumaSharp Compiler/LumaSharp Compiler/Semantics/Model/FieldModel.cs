using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    internal sealed class FieldModel : IFieldSymbol
    {
        // Private
        private readonly SemanticModel model;
        private readonly FieldSyntax fieldSyntax;

        // Properties
        public bool IsConst => fieldSyntax.HasModifier(SyntaxTokenKind.ConstKeyword);
        public bool IsReadOnly => fieldSyntax.HasModifier(SyntaxTokenKind.ReadonlyKeyword);
        public bool IsAssigned => fieldSyntax.HasFieldAssignment;
        public bool IsPublic => fieldSyntax.HasModifier(SyntaxTokenKind.PublicKeyword);
        public bool IsProtected => fieldSyntax.HasModifier(SyntaxTokenKind.ProtectedKeyword);
        public bool IsPrivate => fieldSyntax.HasModifier(SyntaxTokenKind.PrivateKeyword);
        public bool IsInternal => fieldSyntax.HasModifier(SyntaxTokenKind.InternalKeyword);
        public bool IsStatic => fieldSyntax.HasModifier(SyntaxTokenKind.StaticKeyword);
        public string Name => fieldSyntax.Identifier.Text;
        public SyntaxSpan? Source => fieldSyntax.GetSpan();

        // Constructor
        public FieldModel(SemanticModel model, FieldSyntax field)
        {
            this.model = model;
            this.fieldSyntax = field;
        }

        // Methods
        public ITypeSymbol GetDeclaringType()
        {
            // Try to declaring type symbol
            return model.GetSymbol(fieldSyntax.ParentOfType<MemberSyntax>()).Symbol as ITypeSymbol;
        }

        public ITypeSymbol GetFieldType()
        {
            // Get the field type
            return model.GetTypeSymbol(fieldSyntax.FieldType);
        }        
    }
}
