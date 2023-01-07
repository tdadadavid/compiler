namespace compiler.CodeAnalysis
{
  public enum SyntaxKind
  {
    NumberToken,
    WhiteSpaceToken,
    PlusToken,
    SubtractionToken,
    MultiplicationToken,
    DivisionToken,
    OpenParenthesesToken,
    ClosedParenthesesToken,
    BadToken,
    EndOfFileToken,
    NumberExpression,
    BinaryExpression,
    ParenthesizedExpression
  }
}