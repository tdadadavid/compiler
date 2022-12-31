using System.Text.RegularExpressions;

namespace mc
{
    class Program
    {
        static void Main(string[] args)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) Console.WriteLine("not parseable");

            // var parser = new Parser(line);
            // var expression = parser.Parse();

            // var color = Console.ForegroundColor;
            // Console.ForegroundColor = ConsoleColor.DarkGray;

          
            // Console.ForegroundColor = color;
            // var lexer = new Lexer(line);

            while (true)
            {
                // continuously iterate as long as there are tokens
                // get each token check if the token is an EndOfFile Token
                // if true break (parsing ended) else check the value of the 
                // token passed if its null (for symbols) print the token kind
                // and token Text else Print the Kind, Text and Value
                var token = lexer.NextToken();
                if (token.Kind == SyntaxKind.EndOfFile) break;

                Console.WriteLine(token.Value != null
                    ? $"{token.Kind}: {token.Text} {token.Value}"
                    : $"{token.Kind}: {token.Text}");
            }
        }

        static void PrettyPrint(SyntaxNode node, string indent = "")
        {
            Console.Write(node.Kind);

            if (node is SyntaxToken t && t != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }

            indent += "    ";
            foreach(var child in node.GetChildren()) PrettyPrint(child, indent);
        }
    }
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
        BinaryExpression
    }

    class SyntaxToken : SyntaxNode
    {
        public SyntaxToken(SyntaxKind kind, int position, string? text, object? value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }

        public override SyntaxKind Kind { get; }

        public int Position { get; }

        public string? Text { get; }

        public object? Value { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }
    }

    class Lexer
    {
        private readonly string _text;
        private int _position;

        public Lexer(string text)
        {
            _text = text;
        }

        // method to get the current character in the text
        private char Current => IfPositionIsAtEndOfLineOrOutOfTheLine() ? '\n' : _text[_position];

        private void Next()
        {
            _position++;
        }

        // check if the position is out of the text
        private bool IfPositionIsAtEndOfLineOrOutOfTheLine()
        {
            return _position == _text.Length;
        }

        public SyntaxToken NextToken()
        {
            if (_position == _text.Length)
                return new SyntaxToken(SyntaxKind.EndOfFile, _position++, "\0", null);

            if (char.IsDigit(Current))
            {
                var start = _position;
                while (char.IsDigit(Current)) Next();

                var length = _position - start;
                var subStringDigit = _text.Substring(start, length);
                int.TryParse(subStringDigit, out var value);
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

            return Current switch
            {
                '+' => new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null),
                '-' => new SyntaxToken(SyntaxKind.SubtractionToken, _position++, "-", null),
                '*' => new SyntaxToken(SyntaxKind.MultiplicationToken, _position++, "*", null),
                '/' => new SyntaxToken(SyntaxKind.DivisionToken, _position++, "/", null),
                '(' => new SyntaxToken(SyntaxKind.OpenParenthesesToken, _position++, "(", null),
                ')' => new SyntaxToken(SyntaxKind.ClosedParenthesesToken, _position++, ")", null),
                _ => new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null),
            };
        }
    }

    abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public abstract IEnumerable<SyntaxNode> GetChildren();
    }

    abstract class ExpressionSyntax : SyntaxNode { }

    sealed class NumberExpressionSyntax : ExpressionSyntax
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

        private SyntaxToken NumberToken { get; }

    }

    sealed class BinaryExpressionSyntax : ExpressionSyntax
    {

        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken token, ExpressionSyntax right)
        {
            Left = left;
            Token = token;
            Right = right;
        }

        public ExpressionSyntax Left { get; }
        public SyntaxToken Token { get; }
        public ExpressionSyntax Right { get; }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return Token;
            yield return Right;
        }
    }

    class Parser
    {
        private readonly SyntaxToken[] _tokens;
        private int _positions;

        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();
            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.NextToken();

                if (!TokenIsWhiteSpace(token) && !TokenIsBadToken(token)) tokens.Add(token);

            } while (token.Kind != SyntaxKind.EndOfFile);

            _tokens = tokens.ToArray();
        }


        private SyntaxToken NextToken()
        {
            var result = Current;
            _positions++;
            return result;
        }

        public ExpressionSyntax Parse()
        {
            // The Pre-Order Traversal (L-ROOT-R) algorithm is used here 
            // Where Left (l) ==> Operands eg Numbers
            //       Root (ROOT) ==> Operator eg (*,+,-)
            //       Right (r) ==>  Operands eg Numbers
            var left = ParsePrimaryExpression();

            while (TokenIsPlusToken(Current) || TokenIsSubtractionToken(Current))
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }


        private SyntaxToken Match(SyntaxKind kind)
        {
            return Current.Kind == kind ? NextToken() : new SyntaxToken(kind, Current.Position, null, null);
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            var numberToken = Match(SyntaxKind.NumberToken);
            return new NumberExpressionSyntax(numberToken);
        }

        // This method check the particular token at a given point
        // in the List of Token. 
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

        private SyntaxToken Current => Peek(0);

        private bool TokenIsSubtractionToken(SyntaxToken token) => token.Kind == SyntaxKind.SubtractionToken;
        private bool TokenIsPlusToken(SyntaxToken token) => token.Kind == SyntaxKind.PlusToken;
        private bool TokenIsWhiteSpace(SyntaxToken token) => token.Kind == SyntaxKind.WhiteSpaceToken;
        private bool TokenIsBadToken(SyntaxToken token) => token.Kind == SyntaxKind.BadToken;
    }
}