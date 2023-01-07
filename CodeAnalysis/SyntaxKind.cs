namespace compiler.CodeAnalysis
{
  public enum SyntaxKind
  {
    //Tokens
    NumberToken,
    WhiteSpaceToken,
    PlusToken,
    SubtractionToken,
    MultiplicationToken,
    DivisionToken,
    OpenParenthesesToken,
    ClosedParenthesesToken,
    
    //Special tokens
    BadToken,
    EndOfFileToken,
    
    //Expression
    NumberExpression,
    BinaryExpression,
    ParenthesizedExpression
  }
}