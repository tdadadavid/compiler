namespace compiler.CodeAnalysis
{
  sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
  {
    public ParenthesizedExpressionSyntax(SyntaxToken openParenthesizes, ExpressionSyntax expressionSyntax, SyntaxToken closedParenthesizes)
    {
      OpenParenthesizes = openParenthesizes;
      ExpressionSyntax = expressionSyntax;
      ClosedParenthesizes = closedParenthesizes;

    }

    public SyntaxToken OpenParenthesizes { get; }
    public ExpressionSyntax ExpressionSyntax { get; }
    public SyntaxToken ClosedParenthesizes { get; }
    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression; 

    public override IEnumerable<SyntaxNode> GetChildren()
    {
      yield return OpenParenthesizes;
      yield return ExpressionSyntax;
      yield return ClosedParenthesizes;
    }
  }
}