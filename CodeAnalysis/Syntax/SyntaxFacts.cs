namespace compiler.CodeAnalysis.Syntax
{
  static class SyntaxFacts
  {

    public static int GetUnaryPrecedenceForSyntaxKind(this SyntaxKind kind)
    {
      switch (kind)
      {
        case SyntaxKind.PlusToken:
        case SyntaxKind.SubtractionToken:
          return 3;
        
        default:
          return 0;
      }
    }
    
    /**
     * @description This returns the precedence of the given kind
     * of Syntax.
     *
     * For Multiplication and Division, They carry higher precedence
     * compared to Addition and Subtraction.
     */
    
    public static int GetBinaryPrecedenceForSyntaxKind(this SyntaxKind kind)
    {
      switch (kind)
      {
        case SyntaxKind.MultiplicationToken:
        case SyntaxKind.DivisionToken:
          return 2;
        
        case SyntaxKind.PlusToken:
        case SyntaxKind.SubtractionToken:
          return 1;
        
        default:
          return 0;
        
      }
    }
  }
}