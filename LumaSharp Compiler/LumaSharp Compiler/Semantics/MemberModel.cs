using LumaSharp_Compiler.Syntax;

namespace LumaSharp_Compiler.Semantics
{
    public enum AccessModifier
    {
        Export,
        Internal,
        Hidden,
        Global,
    }

    public abstract class MemberModel : ModelNode
    {
        // Private
        private MemberSyntax syntax = null;
        private MemberModel parent = null;
        private AccessModifier[] accessModifiers = null;

        // Properties
        public string MemberName
        {
            get { return syntax.Identifier.Text; }
        }

        public AccessModifier[] AccessModifiers
        {
            get { return accessModifiers; }
        }

        public MemberModel Parent
        {
            get { return parent; }
        }

        public int AccessModifierCount
        {
            get { return HasAccessModifiers ? accessModifiers.Length : 0; }
        }

        public bool HasParent
        {
            get { return parent != null; }
        }

        public bool HasAccessModifiers
        {
            get { return accessModifiers != null; }
        }

        // Constructor
        protected MemberModel(MemberSyntax syntax, SemanticModel model, MemberModel parent)
            : base(model)
        {
            this.syntax = syntax;
            this.parent = parent;

            if(syntax.HasAccessModifiers == true)
                this.accessModifiers = GetAccessModifiers(syntax.AccessModifiers);
        }

        // Methods
        public bool HasAccessModifier(AccessModifier accessModifier)
        {
            return accessModifiers.Contains(accessModifier);
        }

        private AccessModifier[] GetAccessModifiers(SyntaxToken[] modifiers)
        {
            List<AccessModifier> result = new List<AccessModifier>(4);

            for(int i = 0; i < modifiers.Length; i++)
            {
                switch(modifiers[i].Text)
                {
                    case "export": result.Add(AccessModifier.Export); break;
                    case "internal": result.Add(AccessModifier.Internal); break;
                    case "hidden": result.Add(AccessModifier.Hidden); break;
                    case "global": result.Add(AccessModifier.Global); break;
                }
            }

            return result.ToArray();
        }
    }
}
