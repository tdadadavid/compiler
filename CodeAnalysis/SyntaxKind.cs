namespace Minsk.CodeAnaylysis
{
  enum SyntaxKind
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
    EndOfFile,
    NumberExpression,
    BinaryExpression,
    ParenthesizedExpression
  }
}