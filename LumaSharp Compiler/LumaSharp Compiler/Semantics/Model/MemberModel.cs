using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Model
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
        private readonly StringModel identifier;
        private readonly SyntaxToken[] accessModifierTokens;
        private AccessModifier[] accessModifiers = null;

        // Properties
        public string MemberName
        {
            get { return identifier.Text; }
        }

        public AccessModifier[] AccessModifiers
        {
            get { return accessModifiers; }
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

        public bool IsGlobal
        {
            get { return HasAccessModifier(AccessModifier.Global); }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return Model; }
        }

        public abstract _TokenHandle Token { get; }

        // Constructor
        protected MemberModel(SyntaxToken identifier, SyntaxToken[] accessModifiers, SyntaxSpan? span)
            : base(span)
        {
            this.identifier = new StringModel(identifier);
            this.accessModifierTokens = accessModifiers;
        }

        // Methods
        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Check for access modifiers
            if (accessModifiers != null)
            {
                // Check for multiple access modifiers
                if ((HasAccessModifier(AccessModifier.Internal) == true && HasAccessModifier(AccessModifier.Hidden) == true)
                    || (HasAccessModifier(AccessModifier.Internal) == true && HasAccessModifier(AccessModifier.Export) == true)
                    || (HasAccessModifier(AccessModifier.Hidden) == true && HasAccessModifier(AccessModifier.Export) == true))
                {
                    report.ReportDiagnostic(Code.MultipleAccessModifiers, MessageSeverity.Error, syntax.AccessModifiers[0].Span);
                }

                // Check for type
                if ((this is TypeModel) && HasAccessModifier(AccessModifier.Global) == true)
                    report.ReportDiagnostic(Code.AccessModifierNotValid, MessageSeverity.Error, syntax.AccessModifiers.First(m => m.Text == "global").Span, "global");

                // Check for contract or enum parent
                if (this is not TypeModel && parent is TypeModel parentType && (parentType.IsContract == true || parentType.IsEnum == true))
                {
                    foreach (SyntaxToken token in accessModifierTokens)
                    {
                        report.ReportDiagnostic(Code.AccessModifierNotValid, MessageSeverity.Error, token.Span, token.Text);
                    }
                }
            }
        }

        /*public abstract void StaticallyEvaluateMember(ISymbolProvider provider)*/

        public bool HasAccessModifier(AccessModifier accessModifier)
        {
            return accessModifiers.Contains(accessModifier);
        }

        public bool HasAccessModifier(AccessModifier accessModifier, out SyntaxSpan span)
        {
            span = default;
            int index = Array.IndexOf(accessModifiers, accessModifier);

            // Check for modifier
            if (index != -1)
                span = accessModifierTokens[index].Span;

            return index != -1;
        }

        private AccessModifier[] GetAccessModifiers(SyntaxToken[] modifiers)
        {
            List<AccessModifier> result = new List<AccessModifier>(4);

            for (int i = 0; i < modifiers.Length; i++)
            {
                switch (modifiers[i].Kind)
                {
                    case SyntaxTokenKind.ExportKeyword: result.Add(AccessModifier.Export); break;
                    case SyntaxTokenKind.InternalKeyword: result.Add(AccessModifier.Internal); break;
                    case SyntaxTokenKind.HiddenKeyword: result.Add(AccessModifier.Hidden); break;
                    case SyntaxTokenKind.GlobalKeyword: result.Add(AccessModifier.Global); break;
                }
            }

            return result.ToArray();
        }
    }
}
