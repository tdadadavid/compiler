
namespace compiler.CodeAnalysis
{
  public sealed class LiteralExpressionSyntax : ExpressionSyntax
  {
    public LiteralExpressionSyntax(SyntaxToken literalToken)
    {
      LiteralToken = literalToken;
    }

    public override SyntaxKind Kind => SyntaxKind.NumberExpression;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
      yield return LiteralToken;
    }

    public SyntaxToken LiteralToken { get; }

  }
}