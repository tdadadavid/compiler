
namespace compiler.CodeAnalysis
{
  public sealed class NumberExpressionSyntax : ExpressionSyntax
  {
    public NumberExpressionSyntax(SyntaxToken numberToken)
    {
      NumberToken = numberToken;
    }

    public override SyntaxKind Kind => SyntaxKind.NumberExpression;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
      yield return NumberToken;
    }

    public SyntaxToken NumberToken { get; }

  }
}