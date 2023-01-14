
namespace compiler.CodeAnalysis.Syntax
{
  public sealed class LiteralExpressionSyntax : ExpressionSyntax
  {
    public LiteralExpressionSyntax(SyntaxToken literalToken)
    {
      LiteralToken = literalToken;
    }

    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
      yield return LiteralToken;
    }

    public SyntaxToken LiteralToken { get; }

  }
}