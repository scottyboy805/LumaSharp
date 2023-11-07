
using Antlr4.Runtime;

namespace LumaSharp_Compiler.AST
{
    public sealed class VariableDeclarationStatementSyntax : StatementSyntax
    {
        // Private
        private TypeReferenceSyntax variableType = null;
        private SyntaxToken[] identifiers = null;
        private ExpressionSyntax[] assignExpressions = null;
        private SyntaxToken assign = null;
        private SyntaxToken lblock = null;
        private SyntaxToken rblock = null;

        // Properties
        public override SyntaxToken EndToken
        {
            get
            {
                if(AssignExpressionCount > 1)
                {
                    return rblock;
                }
                return base.EndToken;
            }
        }

        public TypeReferenceSyntax VariableType
        {
            get { return variableType; }
        }

        public SyntaxToken[] Identifiers
        {
            get { return identifiers; }
        }

        public SyntaxToken Assign
        {
            get { return assign; }
        }

        public ExpressionSyntax[] AssignExpressions
        {
            get { return assignExpressions; }
        }

        public int IdentifierCount
        {
            get { return identifiers.Length; }
        }

        public int AssignExpressionCount
        {
            get { return HasAssignExpressions ? identifiers.Length : 0; }
        }

        public bool HasAssignExpressions
        {
            get { return assignExpressions != null; }
        }

        // Constructor
        //internal VariableDeclarationStatementSyntax(SyntaxTree tree, SyntaxNode parent, TypeReferenceSyntax variableType, string[] variableNames, ExpressionSyntax[] assignExpressions = null)
        //    : base(tree, parent, new SyntaxToken(";"))
        //{
        //    // Check for incompatible
        //    if (variableNames != null && variableNames.Length > 0 && assignExpressions != null && variableNames.Length != assignExpressions.Length)
        //        throw new ArgumentException("Assign expression length must match variable names length");

        //    this.variableType = variableType;
        //    this.assignExpressions = assignExpressions;

        //    // Setup identifiers
        //    this.identifiers = new SyntaxToken[variableNames.Length];

        //    for(int i = 0; i < variableNames.Length; i++)
        //    {
        //        this.identifiers[i] = new SyntaxToken(variableNames[i]);
        //    }
        //}

        internal VariableDeclarationStatementSyntax(TypeReferenceSyntax variableType, string[] identifiers, ExpressionSyntax[] assignExpressions)
            : base(variableType.StartToken)
        {
            this.variableType = variableType;
            this.identifiers = identifiers.Select(i => new SyntaxToken(i)).ToArray();
            this.identifiers[0].WithLeadingWhitespace(" ");

            this.assignExpressions = assignExpressions;

            assign = SyntaxToken.Assign();
            lblock = SyntaxToken.LBlock();
            rblock = SyntaxToken.RBlock();
        }

        internal VariableDeclarationStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.LocalVariableStatementContext local)
            : base(tree, parent, local)
        {
            // Variable type
            this.variableType = new TypeReferenceSyntax(tree, this, local.typeReference());

            // Identifiers
            this.identifiers = local.IDENTIFIER().Select(i =>  new SyntaxToken(i)).ToArray();

            // Get assignment
            LumaSharpParser.LocalVariableAssignmentContext assignment = local.localVariableAssignment();

            if (assignment != null)
            {
                // Assign expressions
                this.assignExpressions = assignment.expression().Select(e => ExpressionSyntax.Any(tree, this, e)).ToArray();

                // Assign
                this.assign = new SyntaxToken(assignment.assign);

                // Check for block
                if (assignment.lblock != null)
                    this.lblock = new SyntaxToken(assignment.lblock);

                if (assignment.rblock != null)
                    this.rblock = new SyntaxToken(assignment.rblock);
            }
            
            // Semicolon
            this.statementEnd = new SyntaxToken(local.semi);
        }

        internal VariableDeclarationStatementSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ForVariableStatementContext forVariable, IToken semi)
            : base(tree, parent, forVariable)
        {
            // Variable type
            this.variableType = new TypeReferenceSyntax(tree, this, forVariable.typeReference());

            // Identifiers
            this.identifiers = forVariable.IDENTIFIER().Select(i => new SyntaxToken(i)).ToArray();

            // Get assignment
            LumaSharpParser.LocalVariableAssignmentContext assignment = forVariable.localVariableAssignment();

            if(assignment != null)
            {
                // Assign expressions
                this.assignExpressions = assignment.expression().Select(e => ExpressionSyntax.Any(tree, this, e)).ToArray();

                // Assign
                this.assign = new SyntaxToken(assignment.assign);

                // Check for block
                if(assignment.lblock != null)
                    this.lblock= new SyntaxToken(assignment.lblock);

                if(assignment.rblock != null)
                    this.rblock= new SyntaxToken(assignment.rblock);
            }

            // Semicolon
            this.statementEnd = new SyntaxToken(semi);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write type 
            variableType.GetSourceText(writer);

            // Write identifiers
            for(int i = 0; i < identifiers.Length; i++)
            {
                // Identifier
                identifiers[i].GetSourceText(writer);

                // Separator
                if(i < identifiers.Length - 1)
                    writer.Write(",");
            }

            // Check for assignment
            if (HasAssignExpressions == true)
            {
                // Write assign
                assign.GetSourceText(writer);

                // Check for multiple assignments
                if(AssignExpressionCount > 1)
                {
                    // Start block
                    lblock.GetSourceText(writer);

                    // Write all assignments
                    for(int i = 0; i < assignExpressions.Length; i++)
                    {
                        // Write expression
                        assignExpressions[i].GetSourceText(writer);

                        // Write comma
                        if(i < assignExpressions.Length - 1)
                            writer.Write(",");
                    }

                    // End block
                    rblock.GetSourceText(writer);
                }
                else
                {
                    // Write single assignment
                    assignExpressions[0].GetSourceText(writer);

                    
                }
            }

            if (HasAssignExpressions == false || AssignExpressionCount == 1)
            {
                // Write semi colon
                statementEnd.GetSourceText(writer);
            }
        }
    }
}
