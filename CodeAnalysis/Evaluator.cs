namespace compiler.CodeAnalysis
{
  public sealed class Evaluator
  {
    private readonly ExpressionSyntax _root;

    public Evaluator(ExpressionSyntax root)
    {
      _root = root;
    }

    public int Evaluate()
    {
      return EvaluateExpression(_root);
    }

    private int EvaluateExpression(ExpressionSyntax node)
    {
      if (node is LiteralExpressionSyntax n)
        return (int)n.LiteralToken.Value!;

      if (node is BinaryExpressionSyntax b)
      {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);

        var operatorKind = b.OperatorToken.Kind;
        return operatorKind switch
        {
        SyntaxKind.PlusToken => left + right,
        SyntaxKind.DivisionToken => left / right,
        SyntaxKind.MultiplicationToken => left * right,
        SyntaxKind.SubtractionToken => left - right,
        _ => throw new Exception($"Unexpected binary operator {operatorKind}")
        };
      }

      if (node is UnaryExpressionSyntax u)
      {
        var operand = EvaluateExpression(u.Operand); 
        var operatorTokenKind = u.OperatorToken.Kind;

        return operatorTokenKind switch
        {
        SyntaxKind.PlusToken => operand,
        SyntaxKind.SubtractionToken => -operand,
        _ => throw new Exception($"Unexpected node {node.Kind}")
        };
      }

      if (node is ParenthesizedExpressionSyntax p) return EvaluateExpression(p.ExpressionSyntax);
      throw new Exception($"Unexpected node {node.Kind}");
    }
        
  }
}