namespace compiler.CodeAnalysis
{
  class Parser
  {
    private readonly SyntaxToken[] _tokens;
    private int _positions;
    private readonly List<string> _diagnostics = new List<string>();

    public Parser(string text)
    {
      var lexer = new Lexer(text);

      // scan through the input stream and break down into tokens
      // then pass the tokens as an array to the parser.
      // A lexer is also called a scanner.
      _tokens = lexer.ScanThroughText().ToArray(); 

      // add all the errors to the parser.
      _diagnostics.AddRange(lexer.Diagnostic);
    }

    // get all errors 
    public IEnumerable<string> Diagnostics => _diagnostics;

  
    /**
     * @returns the Current Token and shifts the position to the next token
     */
    private SyntaxToken NextToken()
    {
      var result = Current;
      _positions++;
      return result;
    }

    /**
     * 
     */
    public SyntaxTree Parse()
    {
      var expression = ParseTerm();
      var endOfFileToken = Match(SyntaxKind.EndOfFileToken);
      return new SyntaxTree(_diagnostics, expression, endOfFileToken);
    }

    /**
     * @description This simulates the recursive descent parser
     * since am using a while loop right now, I use ParseFactor
     * to bind Binary Expression tightly coupled together by the
     * operator binding them. eg 1 + 2 * 3 = 7 not 9 because "*"
     * has higher precedence over "+", same goes for division "*" and
     * subtraction "-".
     */
    // Parses + and -, that's all I understand for now
    private ExpressionSyntax ParseTerm()
    {
      // The Pre-Order Traversal (L-ROOT-R) algorithm is used here 
      // Where Left (l) ==> Operands eg Numbers
      //       Root (ROOT) ==> Operator eg (*,+,-)
      //       Right (r) ==>  Operands eg Numbers
      var left = ParseFactor();
            
      while (TokenIsPlusToken(Current) || TokenIsSubtractionToken(Current))
      {
        var operatorToken = NextToken();
        var right = ParseFactor();
        left = new BinaryExpressionSyntax(left, operatorToken, right);
      }

      return left;
    }

    /**
     * @description this parses binary expressions bounded by higher
     * precedent operators "*" and "/"
     */
    private ExpressionSyntax ParseFactor()
    {
      // The Pre-Order Traversal (L-ROOT-R) algorithm is used here 
      // Where Left (l) ==> Operands eg Numbers
      //       Root (ROOT) ==> Operator eg (*,+,-)
      //       Right (r) ==>  Operands eg Numbers
      var left = ParsePrimaryExpression();

      while (TokenIsDivisionToken(Current) || TokenIsMultiplicationToken(Current))
      {
        var operatorToken = NextToken();
        var right = ParsePrimaryExpression();
        left = new BinaryExpressionSyntax(left, operatorToken, right);
      }

      return left;
    }


    private ExpressionSyntax ParseExpression()
    {
      return ParseTerm();
    }
    
    /**
     * @description Check if the current kind of token matches the
     * expected token in a Binary expression. For example the expression
     * 
     * ```
     *    2 + 3 Produces the Syntax Tree
     *    Binary Expression
     *        NumberExpression
     *           NumberToken 2
     *        PlusToken + 
     *        NumberExpression
     *           NumberToken 3
     * ```
     * Because our definition of a BinaryExpression (check BinaryExpressionSyntax.cs) is
     * (ExpressionSyntax, operatorToken, ExpressionSyntax)
     *
     * so if we enter 2 +
     *
     * It will throw(practically prints) error saying
     * "ERROR: unexpected token '<EndOfFileToken>' expected '<NumberToken>'"
     * because of the BinaryExpressionSyntax.
     *
     */
    private SyntaxToken Match(SyntaxKind kind)
    {
      if (kind == Current.Kind) return NextToken();

      _diagnostics.Add($"ERROR: unexpected token '<{Current.Kind}>' expected '<{kind}>'");
      return new SyntaxToken(kind, Current.Position, null, null);
    }

    /**
     * @description Parses Primary Expressions such as ["(", ")", "+"]
     * First it checks if the expression (input) is a open parenthesis
     * if its true it goes for the next token then parses the remaining
     * expression recursively. At the end it checks if the right closing
     * parenthesis is given eg
     * ```
     *  ((2)) the Binary Tree is
     *      ParenthesizedExpression
     *         OpenParenthesis
     *            ParenthesizedExpression
     *                OpenParenthesis
     *                  NumberExpression
     *                    NumberToken 2
     *                ClosedParenthesis
     *         ClosedParenthesis
     * ```
     * if not true then Its a BinaryExpression not a ParenthesizedExpression
     * It check if the expression is a NumberToken if True
     * return a NumberExpression else throw an error
     *
     * "ERROR: unexpected token '<token kind>' expected <NumberToken>
     *
     * ```
     *   2 + will provide
     *     BinaryExpression
     *        NumberExpression
     *          NumberToken 2
     *        PlusToken
     * "ERROR: unexpected token '<EndOfFileToken>' expected '<NumberToken>'
     * ```
     * The error will be thrown because a BinaryExpression is a + b
     * where a & b are binary expressions.
     * But what was provided was a
     *  NumberExpression<NumberToken<2>> PlusToken<+> EndOfFileToken<>
     * which does not match the Binary Expression Definition
     *  NumberExpression<NumberToken<value>> OperatorToken<+,-,*,/> NumberExpression<NumberToken<>>
     */
    private ExpressionSyntax ParsePrimaryExpression()
    {
      if (Current.Kind == SyntaxKind.OpenParenthesesToken)
      {
        var left = NextToken();
        var expression = ParseExpression();
        var right = Match(SyntaxKind.ClosedParenthesesToken);
        return new ParenthesizedExpressionSyntax(left, expression, right);
      }
            
      var numberToken = Match(SyntaxKind.NumberToken);
      return new NumberExpressionSyntax(numberToken);
    }

    /**
     * @description Checks the token a desired point in
     * the token list.
     * if the offset given causes the pointer to go outside
     * the token list then return the last token in the list
     * else return the token at the desired location.
     */
    private SyntaxToken Peek(int offset)
    {
      var index = _positions + offset;
      // if the position is outside the length of the array of tokens
      // return the last element in the array else just return the 
      // element in the required index.
      return index >= _tokens.Length
      ? _tokens[^1]  // return last element (c# lingo).
      : _tokens[index];
    }

    /**
     * @description get the current token.
     */
    private SyntaxToken Current => Peek(0);

    /**
     * @description Boolean methods to check the given type of token.
     */
    private bool TokenIsMultiplicationToken(SyntaxToken token) => token.Kind == SyntaxKind.MultiplicationToken;
    private bool TokenIsDivisionToken(SyntaxToken token) => token.Kind == SyntaxKind.DivisionToken;
    private bool TokenIsSubtractionToken(SyntaxToken token) => token.Kind == SyntaxKind.SubtractionToken;
    private bool TokenIsPlusToken(SyntaxToken token) => token.Kind == SyntaxKind.PlusToken;
  }
}