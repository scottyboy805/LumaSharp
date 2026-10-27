using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.AST.Visitor;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics
{
    internal sealed partial class SemanticAnalysis : SyntaxWalker
    {
        // Private
        private readonly SemanticModel model;
        private readonly IDiagnosticReporter diagnostics;

        // Constructor
        public SemanticAnalysis(SemanticModel model, IDiagnosticReporter diagnostics)
        {
            this.model = model;
            this.diagnostics = diagnostics;
        }

        // Methods
        #region Member
        public override void VisitClass(ClassSyntax _class)
        {
            // Get type symbol
            ITypeSymbol typeSymbol = model.GetSymbol(_class).Symbol as ITypeSymbol;

            // Check modifiers
            if (_class.GetModifier(SyntaxTokenKind.ConstKeyword, out SyntaxToken modifier) == true)
                diagnostics.Report(Code.ModifierNotValid, DiagnosticSeverity.Error, modifier.Span, modifier.Text);

            // Check for base types
            if (_class.HasBaseTypes == true)
            {
                // Get base types
                SymbolInfo baseTypeSymbols = model.GetSymbol(_class.BaseTypes);

                // Check base type location
                int baseClassCount = 0;
                for(int i = 0; i < baseTypeSymbols.CandidateSymbols.Count; i++)
                {
                    // Get base type symbol
                    ITypeSymbol baseTypeSymbol = baseTypeSymbols.CandidateSymbols[i] as ITypeSymbol;

                    // Check for symbol resolved
                    if (baseTypeSymbol == null)
                        continue;

                    // Check base type location
                    if (baseTypeSymbol.IsClass == true)
                    {
                        // Check for base type not in first position
                        if (i > 0)
                        {
                            // Base class must come first before any interfaces
                            diagnostics.Report(Code.FirstBaseType, DiagnosticSeverity.Error, _class.BaseTypes[i].GetSpan(), baseTypeSymbol.Name);
                        }
                        baseClassCount++;
                    }

                    // Check for invalid base
                    if(baseTypeSymbol.IsEnum == true || baseTypeSymbol.IsStruct == true)
                    {
                        // Check for first position
                        if(i == 0)
                        {
                            // Invalid base struct or enum
                            diagnostics.Report(baseTypeSymbol.IsEnum ? Code.InvalidEnumBaseType : Code.InvalidStructBaseType, DiagnosticSeverity.Error, _class.BaseTypes[i].GetSpan(), baseTypeSymbol.Name);
                        }
                        else
                        {
                            // Cannot implement struct or enum
                            diagnostics.Report(baseTypeSymbol.IsEnum ? Code.InvalidEnumBaseInterface : Code.InvalidStructBaseInterface, DiagnosticSeverity.Error, _class.BaseTypes[i].GetSpan(), baseTypeSymbol.Name);
                        }
                    }
                }

                // Check for multiple base classes
                if (baseClassCount > 1)
                    diagnostics.Report(Code.MultipleBaseTypes, DiagnosticSeverity.Error, _class.BaseTypes.GetSpan());
            }

            // Visit children
            base.VisitClass(_class);
        }

        public override void VisitField(FieldSyntax field)
        {
            // Get field symbol
            IFieldSymbol fieldSymbol = model.GetSymbol(field).Symbol as IFieldSymbol;

            // Check modifiers - virtual and override not allowed
            if (field.GetModifier(SyntaxTokenKind.VirtualKeyword, out SyntaxToken modifier) == true || field.GetModifier(SyntaxTokenKind.OverrideKeyword, out modifier) == true)
                diagnostics.Report(Code.ModifierNotValid, DiagnosticSeverity.Error, modifier.Span, modifier.Text);

            // Check assignment type
            if (field.HasFieldAssignment == true)
            {
                // Check for type
                ITypeSymbol fieldTypeSymbol = fieldSymbol.GetFieldType();

                // Get assignment type
                ITypeSymbol assignTypeSymbol = model.GetTypeSymbol(field.FieldAssignment);

                // Check for assignable - if either type is not resolved we cannot perform this check and will report missing type insead
                if (fieldTypeSymbol != null && fieldTypeSymbol != assignTypeSymbol)
                {
                }

                // Check for struct parent
                MemberSyntax parent = field.ParentOfType<MemberSyntax>();

                // Cannot assign a value directly to a struct field
                if (parent is StructSyntax)
                    diagnostics.Report(Code.FieldCannotBeAssigned, DiagnosticSeverity.Error, field.FieldAssignment.GetSpan(), field.Identifier.Text);
            }
            else
            {
                // Check for const - must be assigned 
                if (field.HasModifier(SyntaxTokenKind.ConstKeyword) == true)
                    diagnostics.Report(Code.FieldRequiresAssign, DiagnosticSeverity.Error, field.GetSpan(), field.Identifier.Text);
            }

            // Visit member
            VisitMember(field);

            // Visit children
            base.VisitField(field);
        }

        private void VisitMember(MemberSyntax member)
        {
            // Check for repeated modifiers
            if(member.HasModifiers == true)
            {
                foreach(SyntaxToken modifier in member.Modifiers)
                {
#warning Probably needs fixing to avoid report the same issue multiple times
                    if(member.Modifiers.Count(m => m.Kind == modifier.Kind) > 1)
                        diagnostics.Report(Code.MultipleModifiers, DiagnosticSeverity.Error, member.GetSpan(), member.Identifier.Text);
                }
            }

            // Check for declared in a static type
            MemberSyntax parentMember = member.ParentOfType<MemberSyntax>();

            // Check for static - static is only valid for class so that is all we need to check
            if (parentMember.HasModifier(SyntaxTokenKind.StaticKeyword) == true && member.HasModifier(SyntaxTokenKind.StaticKeyword) == false && parentMember is ClassSyntax)
                diagnostics.Report(Code.FieldMustBeStatic, DiagnosticSeverity.Error, member.GetSpan(), member.Identifier.Text);
        }
        #endregion

        #region Statement
        public override void VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax variableDeclarationStatement)
        {
            base.VisitVariableDeclarationStatement(variableDeclarationStatement);
        }

        public override void VisitIfStatement(IfStatementSyntax ifStatement)
        {
            // Get expected condition type - bool
            ITypeSymbol boolSymbol = model.Binder.GetReservedType(ReservedType.Bool);

            // Get type
            ITypeSymbol conditionTypeSymbol = model.GetTypeSymbol(ifStatement.Condition);

            // Check for type resolved
            if (conditionTypeSymbol != null)
            {
                // Check expression can be converted to bool
                CheckImplicitConversion(conditionTypeSymbol, boolSymbol, ifStatement.Condition.GetSpan());
            }

            // Visit children
            base.VisitIfStatement(ifStatement);
        }

        public override void VisitWhileStatement(WhileStatement whileStatement)
        {
            // Get expected conditional type - bool
            ITypeSymbol boolSymbol = model.Binder.GetReservedType(ReservedType.Bool);

            // Get type
            ITypeSymbol conditionTypeSymbol = model.GetTypeSymbol(whileStatement.Expression);

            // Check for type resolved
            if(conditionTypeSymbol != null)
            {
                // Check expression can be converted to bool
                CheckImplicitConversion(conditionTypeSymbol, boolSymbol, whileStatement.Expression.GetSpan());
            }

            // Visit children
            base.VisitWhileStatement(whileStatement);
        }
        #endregion

        #region Expression
        public override void VisitBaseExpression(BaseExpressionSyntax baseExpression)
        {
            // Try to resolve type
            ITypeSymbol baseTypeSymbol = model.GetSymbol(baseExpression).Symbol as ITypeSymbol;

            // Get declaring method
            IMethodSymbol usedMethodSymbol = model.GetSymbol(baseExpression.ParentOfType<MemberSyntax>()).Symbol as IMethodSymbol;

            // Check for null or used in a static method
            if (baseTypeSymbol == null || usedMethodSymbol == null || usedMethodSymbol.IsStatic == true)
                diagnostics.Report(Code.KeywordNotValid, DiagnosticSeverity.Error, baseExpression.GetSpan(), SyntaxToken.GetText(SyntaxTokenKind.BaseKeyword));
        }

        public override void VisitThisExpression(ThisExpressionSyntax thisExpression)
        {
            // Try to resolve type
            ITypeSymbol thisTypeSymbol = model.GetSymbol(thisExpression).Symbol as ITypeSymbol;

            // Get declaring method
            IMethodSymbol usedMethodSymbol = model.GetSymbol(thisExpression.ParentOfType<MemberSyntax>()).Symbol as IMethodSymbol;

            // Check for null or used in a static method
            if (thisTypeSymbol == null || usedMethodSymbol == null || usedMethodSymbol.IsStatic == true)
                diagnostics.Report(Code.KeywordNotValid, DiagnosticSeverity.Error, thisExpression.GetSpan(), SyntaxToken.GetText(SyntaxTokenKind.ThisKeyword));
        }

        public override void VisitVariableReferenceExpression(VariableReferenceExpressionSyntax variableReferenceExpression)
        {
            // Try to resolve reference
            ISymbol variableSymbol = model.GetSymbol(variableReferenceExpression).Symbol;

            // Check for symbol found
            if (variableSymbol == null)
                diagnostics.Report(Code.IdentifierNotFound, DiagnosticSeverity.Error, variableSymbol.Source, variableReferenceExpression.Identifier.Text);
        }
        #endregion
    }
}
