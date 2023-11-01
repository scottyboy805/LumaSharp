using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Model
{
    public enum AccessModifier
    {
        Export,
        Internal,
        Hidden,
        Global,
    }

    public abstract class MemberModel : SymbolModel, IReferenceSymbol
    {
        // Private
        private MemberSyntax syntax = null;
        private MemberModel parent = null;
        private AccessModifier[] accessModifiers = null;

        // Internal
        internal int memberToken = -1;

        // Properties
        public string MemberName
        {
            get { return syntax.Identifier.Text; }
        }

        public AccessModifier[] AccessModifiers
        {
            get { return accessModifiers; }
        }

        public new MemberModel Parent
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

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return Model; }
        }

        public int SymbolToken
        {
            get { return memberToken; }
        }

        // Constructor
        protected MemberModel(SemanticModel model, SymbolModel parent, MemberSyntax syntax)
            : base(model, parent)
        {
            this.syntax = syntax;
            
            if(parent is MemberModel)
                this.parent = parent as MemberModel;   

            if (syntax.HasAccessModifiers == true)
                accessModifiers = GetAccessModifiers(syntax.AccessModifiers);
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Check for access modifiers
            if (accessModifiers != null)
            {
                // Check for multiple access modifiers
                if ((accessModifiers.Contains(AccessModifier.Internal) == true && accessModifiers.Contains(AccessModifier.Hidden) == true)
                    || (accessModifiers.Contains(AccessModifier.Internal) == true && accessModifiers.Contains(AccessModifier.Export) == true)
                    || (accessModifiers.Contains(AccessModifier.Hidden) == true && accessModifiers.Contains(AccessModifier.Export) == true))
                {
                    report.ReportMessage(Code.MultipleAccessModifiers, MessageSeverity.Error, syntax.AccessModifiers[0].Source);
                }

                // Check for type
                if ((this is TypeModel) && accessModifiers.Contains(AccessModifier.Global) == true)
                    report.ReportMessage(Code.AccessModifierNotValid, MessageSeverity.Error, syntax.AccessModifiers.First(m => m.Text == "global").Source, "global");

                // Check for contract or enum parent
                if (parent is TypeModel && ((TypeModel)parent).IsContract == true || ((TypeModel)parent).IsEnum == true)
                {
                    foreach (SyntaxToken token in syntax.AccessModifiers)
                    {
                        report.ReportMessage(Code.AccessModifierNotValid, MessageSeverity.Error, token.Source, token.Text);
                    }
                }
            }
        }

        public bool HasAccessModifier(AccessModifier accessModifier)
        {
            return accessModifiers.Contains(accessModifier);
        }

        private AccessModifier[] GetAccessModifiers(SyntaxToken[] modifiers)
        {
            List<AccessModifier> result = new List<AccessModifier>(4);

            for (int i = 0; i < modifiers.Length; i++)
            {
                switch (modifiers[i].Text)
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
