namespace compiler.CodeAnalysis
{
  public sealed class SyntaxTree {

    public SyntaxTree(IEnumerable<string> diagnostics, ExpressionSyntax root, SyntaxToken endOfFileToken)
    {
      Diagnostics = diagnostics.ToArray();
      Root = root;
      EndOfFileToken = endOfFileToken;
    }

    public static SyntaxTree Parse(string text)
    {
      return new Parser(text).Parse();
    }
        
    public IReadOnlyList<string> Diagnostics { get; }
    public ExpressionSyntax Root { get; }
    public SyntaxToken EndOfFileToken { get; }
  }
}