namespace compiler.CodeAnalysis
{
  class Lexer
  {
    private readonly string _text;
    private int _position;
    private readonly List<string> _diagnostics = new List<string>(); // this is for handling errors

    public Lexer(string text)
    {
      _text = text;
    }

    public IEnumerable<string> Diagnostic => _diagnostics;
    
    // Bola is a boy
    private char Current => PositionIsAtEndOfLineOrOutOfTheLine() ? '\n' : _text[_position];

    private void Next()
    {
      _position++;
    }

    private bool PositionIsAtEndOfLineOrOutOfTheLine()
    {
      return _position == _text.Length;
    }

    public SyntaxToken NextToken()
    {
      // check if we have reached the end of the text (line)
      if (PositionIsAtEndOfLineOrOutOfTheLine())
        return new SyntaxToken(SyntaxKind.EndOfFileToken, _position++, "\0", null);

      // check if the current string is a digit
      if (char.IsDigit(Current))
      {
        var start = _position;
        while (char.IsDigit(Current)) Next();

        var length = _position - start;
        var subStringDigit = _text.Substring(start, length);
        if(!int.TryParse(subStringDigit, out var value)) 
          _diagnostics.Add($"ERROR: expression {subStringDigit} cannot be represented in int32");
        else
          return new SyntaxToken(SyntaxKind.NumberToken, start, subStringDigit, value);
      }

      if (char.IsWhiteSpace(Current))
      {
        var start = _position;

        while (char.IsWhiteSpace(Current)) Next();
        var length = _position - start;
        var whiteSpaceSubString = _text.Substring(start, length);
        return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, whiteSpaceSubString, null);
      }

      switch (Current)
      {
        case '+':
          return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
        case '-':
          return new SyntaxToken(SyntaxKind.SubtractionToken, _position++, "-", null);
        case '*':
          return new SyntaxToken(SyntaxKind.MultiplicationToken, _position++, "*", null);
        case '/':
          return new SyntaxToken(SyntaxKind.DivisionToken, _position++, "/", null);
        case '(':
          return new SyntaxToken(SyntaxKind.OpenParenthesesToken, _position++, "(", null);
        case ')':
          return new SyntaxToken(SyntaxKind.ClosedParenthesesToken, _position++, ")", null);
        default:
          _diagnostics.Add($"ERROR: bad character input '{Current}'");
          return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);
      }
    }
  }
}