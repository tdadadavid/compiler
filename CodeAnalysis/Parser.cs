namespace compiler.CodeAnalysis
{
  internal sealed class Parser
  {
    private readonly SyntaxToken[] _tokens;
    private int _pointer;
    private readonly List<string> _diagnostics = new List<string>();

    public Parser(string text)
    {
      var lexer = new Lexer(text);

      // scan through the input stream and break down into tokens
      // then pass the tokens as an array to the parser.
      // A lexer is also called a scanner.
      _tokens = lexer.ScanThroughText(); 

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
      _pointer++;
      return result;
    }

    /**
     * 
     */
    public SyntaxTree Parse()
    {
      var expression = ParseExpression();
      var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
      return new SyntaxTree(_diagnostics, expression, endOfFileToken);
    }
    
    /**
     * @description:
     *  This methods parses any given array of SyntaxToken and evaluates
     *  them using "Operator Precedence'
     *
     *  It accepts a default value called 'precedence' to take account of the
     *  last operator precedence. By default it is 'Zero' because.
     *
     *  Lets work through an example:
     *    Given: 200 - 80 / 2
     *  parsing this into a syntax tree format we'll have
     *
     *  ```
     *    BinaryExpression
     *      LiteralExpression
     *          NumberToken    200
     *      SubtractionToken    -
     *      BinaryExpression
     *          LiteralExpression
     *             NumberToken  80
     *          DivisionToken   /
     *          LiteralExpression
     *             NumberToken  2
     * ```
     *
     * How this is achieved is that
     *  First:
     *    - We parse the first token from our Lexer (lexer.ScanThroughText()) which is a NumberToken<1>
     *    so "left" = SyntaxToken<NumberToken<1>>
     *  Second:
     *    - Now start a loop
     *    - Get the precedence of the current token by checking the its precedence in the static method
     *     `GetPrecedenceForSyntaxKind` so the current SyntaxToken is a NumberToken and its precedence is
     *     `0` (actually precedence has to do with operators <+,-,*,/>
     *    - Store the currentPrecedence as `0`
     *  Third:
     *    - Check two necessary conditions
     *      a. Is this a valid `BinaryExpression Operator` ?
     *      b. Is the previousPrecedence(0) lower than the currentPrecedence(0)
     *
     *      Answering a.
     *        If we look at the GetPrecedenceForSyntaxKind we see that any token that is
     *        not [+, -, *, /] we return 0,Then the helper method `CurrentOperatorIsBinaryExpressionOperator`
     *        checks if it's a BinaryExpression Operator and returns false as the answer
     *     Answering b.
     *        This is simple, just ask is currentPrecedence >= previousPrecedence (0 >= 0) which is true.
     *    - So True or False ==> True.
     *    **The evaluation stops as the break statement is called and moves to the line**      
     */

    private ExpressionSyntax ParseExpression(int previousPrecedence = 0)
    {
      var left = ParsePrimaryExpression();

      while (true)
      {
        var currentPrecedence = GetPrecedenceForSyntaxKind(Current.Kind);
        if (
            !CurrentOperatorIsBinaryExpressionOperator(currentPrecedence) || 
            CurrentOperatorTokenHasHigherPrecedenceThanPreviousOperatorToken(previousPrecedence, currentPrecedence)
          ) break;

        var operatorToken = NextToken();
        var right = ParseExpression(currentPrecedence);
        left = new BinaryExpressionSyntax(left, operatorToken, right);
      }

      return left;
    }

    /**
     * @description This returns the precedence of the given kind
     * of Syntax.
     *
     * For Multiplication and Division, They carry higher precedence
     * compared to Addition and Subtraction.
     */
    private static int GetPrecedenceForSyntaxKind(SyntaxKind kind)
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
    
    /**
     * @description Check if the current kind of token matches the
     * expected token in a Binary expression. For example the expression
     * 
     * ```
     *    2 + 3 Produces the Syntax Tree
     *    Binary Expression
     *        LiteralExpression
     *           NumberToken 2
     *        PlusToken + 
     *        LiteralExpression
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
    private SyntaxToken MatchToken(SyntaxKind kind)
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
     *                  LiteralExpression
     *                    NumberToken 2
     *                ClosedParenthesis
     *         ClosedParenthesis
     * ```
     * if not true then Its a BinaryExpression not a ParenthesizedExpression
     * It check if the expression is a NumberToken if True
     * return a LiteralExpression else throw an error
     *
     * "ERROR: unexpected token '<token kind>' expected <NumberToken>
     *
     * ```
     *   2 + will provide
     *     BinaryExpression
     *        LiteralExpression
     *          NumberToken 2
     *        PlusToken
     * "ERROR: unexpected token '<EndOfFileToken>' expected '<NumberToken>'
     * ```
     * The error will be thrown because a BinaryExpression is a + b
     * where a & b are binary expressions.
     * But what was provided was a
     *  LiteralExpression<NumberToken<2>> PlusToken<+> EndOfFileToken<>
     * which does not match the Binary Expression Definition
     *  LiteralExpression<NumberToken<value>> OperatorToken<+,-,*,/> LiteralExpression<NumberToken<>>
     */
    private ExpressionSyntax ParsePrimaryExpression()
    {
      if (Current.Kind == SyntaxKind.OpenParenthesesToken)
      {
        var left = NextToken();
        var expression = ParseExpression();
        var right = MatchToken(SyntaxKind.ClosedParenthesesToken);
        return new ParenthesizedExpressionSyntax(left, expression, right);
      }
            
      var numberToken = MatchToken(SyntaxKind.NumberToken);
      return new LiteralExpressionSyntax(numberToken);
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
      var index = _pointer + offset;
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
    
    private bool CurrentOperatorIsBinaryExpressionOperator(int precedence) => precedence != 0;

    private bool CurrentOperatorTokenHasHigherPrecedenceThanPreviousOperatorToken(int previous, int current) =>
    previous >= current;
  }
}