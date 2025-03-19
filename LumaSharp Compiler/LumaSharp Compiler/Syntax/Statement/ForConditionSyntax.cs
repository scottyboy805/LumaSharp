
//namespace LumaSharp.Compiler.AST
//{
//    public sealed class ForConditionSyntax : SyntaxNode
//    {
//        // Private
//        private readonly VariableDeclarationStatementSyntax variable;
//        private readonly ExpressionSyntax condition;
//        private readonly SeparatedListSyntax<ExpressionSyntax> increments;
//        private readonly SyntaxToken variableColon;
//        private readonly SyntaxToken conditionColon;

//        // Properties
//        public override SyntaxToken StartToken
//        {
//            get
//            {
//                // Check for variable
//                if (HasVariable == true)
//                    return variable.StartToken;

//                return variableColon;
//            }
//        }

//        public override SyntaxToken EndToken
//        {
//            get
//            {
//                // Check for increment
//                if(HasIncrements == true)
//                    return increments.EndToken;

//                return conditionColon;
//            }
//        }

//        public VariableDeclarationStatementSyntax Variable
//        {
//            get { return variable; }
//        }

//        public ExpressionSyntax Condition
//        {
//            get { return condition; }
//        }

//        public SeparatedListSyntax<ExpressionSyntax> Increments
//        {
//            get { return increments; }
//        }

//        public SyntaxToken VariableColon
//        {
//            get { return variableColon; }
//        }

//        public SyntaxToken ConditionColon
//        {
//            get { return conditionColon; }
//        }

//        public bool HasVariable
//        {
//            get { return variable != null; }
//        }

//        public bool HasCondition
//        {
//            get { return condition != null; }
//        }

//        public bool HasIncrements
//        {
//            get { return increments != null; }
//        }

//        internal override IEnumerable<SyntaxNode> Descendants
//        {
//            get
//            {
//                // Variable
//                if (HasVariable == true)
//                    yield return variable;

//                // Condition
//                if (HasCondition == true)
//                    yield return condition;

//                // Increment
//                if (HasIncrements == true)
//                    yield return increments;
//            }
//        }

//        // Constructor
//        internal ForConditionSyntax(SyntaxNode parent, LumaSharpParser.ForConditionContext forCondition)
//            : base(parent)
//        {
//            // Variable
//            LumaSharpParser.LocalVariableStatementContext localVariable = forCondition.localVariableStatement();

//            // Check for present
//            if (localVariable != null)
//                variable = new VariableDeclarationStatementSyntax(this, localVariable);


//            // Condition
//            LumaSharpParser.ExpressionContext condition = forCondition.expression();

//            // Check for present
//            if (condition != null)
//                this.condition = ExpressionSyntax.Any(this, condition);


//            // Increment
//            LumaSharpParser.ExpressionListContext increments = forCondition.expressionList();

//            // Check for present
//            if (increments != null)
//                this.increments = ExpressionSyntax.List(this, increments);

//            // Get commas
//            variableColon = new SyntaxToken(SyntaxTokenKind.CommaSymbol, forCondition.COLON(0));
//            conditionColon = new SyntaxToken(SyntaxTokenKind.CommaSymbol, forCondition.COLON(1));
//        }

//        // Methods
//        public override void GetSourceText(TextWriter writer)
//        {
//            // Variable
//            if (HasVariable == true)
//                variable.GetSourceText(writer);

//            // Variable comma
//            variableColon.GetSourceText(writer);

//            // Condition
//            if (HasCondition == true)
//                condition.GetSourceText(writer);

//            // Condition comma
//            conditionColon.GetSourceText(writer);

//            // Increment
//            if (HasIncrements == true)
//                increments.GetSourceText(writer);
//        }
//    }
//}
