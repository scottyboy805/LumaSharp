using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Model
{
    public sealed class LocalOrParameterModel : SymbolModel, ILocalIdentifierReferenceSymbol
    {
        // Type
        [Flags]
        public enum Attributes : uint
        {
            Local = 1 << 1,
            Parameter = 1 << 2,

            ByRef = 1 << 6,
            Optional = 1 << 7,
        }

        // Private
        private readonly StringModel identifier;
        private readonly TypeReferenceModel typeModel;
        private readonly ExpressionModel assignModel;
        private readonly Attributes attributes;
        private int index = 0;

        // Properties
        public string IdentifierName
        {
            get { return identifier.Text; }
        }

        public StringModel Identifier
        {
            get { return identifier; }
        }

        public TypeReferenceModel Type
        {
            get { return typeModel; }
        }

        public ExpressionModel Assign
        {
            get { return assignModel; }
        }

        public bool IsLocal
        {
            get { return (attributes & Attributes.Local) != 0; }
        }

        public bool IsParameter
        {
            get { return (attributes & Attributes.Parameter) != 0; }
        }

        public bool IsByReference
        {
            get { return (attributes & Attributes.ByRef) != 0; }
        }

        public bool IsOptional
        {
            get { return (attributes & Attributes.Optional) != 0; }
        }

        public int Index
        {
            get { return index; }
        }

        public ITypeReferenceSymbol TypeSymbol
        {
            get { return typeModel.EvaluatedTypeSymbol; }
        }

        public ILibraryReferenceSymbol LibrarySymbol
        {
            get { return Model.LibrarySymbol; }
        }

        public _TokenHandle Token
        {
            get { return default; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                yield return typeModel;

                // Check for assign
                if (assignModel != null)
                    yield return assignModel;
            }
        }

        // Constructor
        internal LocalOrParameterModel(ParameterSyntax parameterSyntax, int index)
            : base(parameterSyntax != null ? parameterSyntax.GetSpan() : null)
        {
            // Check for null
            if(parameterSyntax == null)
                throw new ArgumentNullException(nameof(parameterSyntax));

            this.identifier = new StringModel(parameterSyntax.Identifier);
            this.typeModel = new TypeReferenceModel(parameterSyntax.ParameterType);
            this.assignModel = parameterSyntax.Assignment != null
                ? ExpressionModel.Any(parameterSyntax.Assignment.AssignExpressions.First(), this)
                : null;
            this.index = index;

            // Set parent
            this.typeModel.parent = this;
        }

        internal LocalOrParameterModel(SyntaxToken identifier, TypeReferenceModel type, ExpressionModel assign, int index, SyntaxSpan? span)
            : base(span)
        {
            // Check for invalid
            if (string.IsNullOrEmpty(identifier.Text) == true)
                throw new ArgumentException(nameof(identifier) + " cannot be null or empty");

            // Check for null
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            this.identifier = new StringModel(identifier);
            this.typeModel = type;
            this.assignModel = assign;
            this.index = index;

            // Set parent
            type.parent = this;

            if (assign != null)
                assign.parent = this;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitLocalOrParameter(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve type
            typeModel.ResolveSymbols(provider, report);

            // Resolve assign
            if(assignModel != null)
                assignModel.ResolveSymbols(provider, report);
        }

        public static LocalOrParameterModel[] CreateLocalVariables(VariableDeclarationSyntax variable, SymbolModel parent)
        {
            // Check for null
            if(variable == null)
                throw new ArgumentNullException(nameof(variable));

            // Create type model
            TypeReferenceModel variableType = new TypeReferenceModel(variable.VariableType);
            variableType.parent = parent;

            // Create locals
            return variable.Identifiers.Select((identifier, index) =>
            {
                // Check for assign
                ExpressionModel assign = variable.HasAssignment == true && index < variable.Assignment.AssignExpressions.Count
                    ? ExpressionModel.Any(variable.Assignment.AssignExpressions[index], parent)
                    : null;

                // Create local
                return new LocalOrParameterModel(identifier, variableType, assign, index, variable.GetSpan());
            }).ToArray();
        }
    }
}
