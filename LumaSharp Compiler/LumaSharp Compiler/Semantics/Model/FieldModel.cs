using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    internal sealed class FieldModel : IFieldSymbol
    {
        // Private
        private readonly SemanticModel model;
        private readonly FieldSyntax fieldSyntax;

        // Properties
        public bool IsConst => fieldSyntax.Modifiers.Any(m => m.Kind == SyntaxTokenKind.ConstKeyword);
        public bool IsReadOnly => fieldSyntax.Modifiers.Any(m => m.Kind == SyntaxTokenKind.ReadonlyKeyword);
        public bool IsAssigned => fieldSyntax.HasFieldAssignment;
        public bool IsPublic => fieldSyntax.Modifiers.Any(m => m.Kind == SyntaxTokenKind.PublicKeyword);
        public bool IsProtected => fieldSyntax.Modifiers.Any(m => m.Kind == SyntaxTokenKind.ProtectedKeyword);
        public bool IsPrivate => fieldSyntax.Modifiers.Any(m => m.Kind == SyntaxTokenKind.PrivateKeyword);
        public bool IsInternal => fieldSyntax.Modifiers.Any(m => m.Kind == SyntaxTokenKind.InternalKeyword);
        public bool IsStatic => fieldSyntax.Modifiers.Any(m => m.Kind == SyntaxTokenKind.StaticKeyword);
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
