using System.Text.RegularExpressions;

namespace mc
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) break;

                var parser = new Parser(line);
                var syntaxTree = parser.Parse();
            
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkBlue;
            
                PrettyPrint(syntaxTree.Root);

                if (syntaxTree.Diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    foreach (var diagnostic in syntaxTree.Diagnostics)
                    {
                        Console.WriteLine(diagnostic);
                    }
                }
                else
                {
                    var evaluator = new Evaluator(syntaxTree.Root);
                    var result = evaluator.Evaluate();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"result = {result}");
                }
            
                Console.ForegroundColor = color;
                
            }
            
        }

        static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";
            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);

            if (node is SyntaxToken t)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }

            Console.WriteLine();

            indent += isLast ? "    " : "│   ";
            var lastChild = node.GetChildren().LastOrDefault();
            
            foreach (var child in node.GetChildren()) PrettyPrint(child, indent, child == lastChild);
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
        private readonly List<string> _diagnostics = new List<string>(); // this is for handling errors

        public Lexer(string text)
        {
            _text = text;
        }

        public IEnumerable<string> Diagnostic => _diagnostics;
        
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
    
    sealed class SyntaxTree {

        public SyntaxTree(IEnumerable<string> diagnostics, ExpressionSyntax root, SyntaxToken endOfFileToken)
        {
            Diagnostics = diagnostics.ToArray();
            Root = root;
            EndOfFileToken = endOfFileToken;
        }
        
        public IReadOnlyList<string> Diagnostics { get; }
        public ExpressionSyntax Root { get; }
        public SyntaxToken EndOfFileToken { get; }
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

        public SyntaxToken NumberToken { get; }

    }

    sealed class BinaryExpressionSyntax : ExpressionSyntax
    {

        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }

        public ExpressionSyntax Left { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Right { get; }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }

    class Parser
    {
        private readonly SyntaxToken[] _tokens;
        private int _positions;
        private readonly List<string> _diagnostics = new List<string>();

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
            _diagnostics.AddRange(lexer.Diagnostic);
        }

        public IEnumerable<string> Diagnostics => _diagnostics;

        private SyntaxToken NextToken()
        {
            var result = Current;
            _positions++;
            return result;
        }

        public SyntaxTree Parse()
        {
            var expression = parseExpression();
            var endOfFileToken = Match(SyntaxKind.EndOfFile);
            return new SyntaxTree(_diagnostics, expression, endOfFileToken);
        }

        private ExpressionSyntax parseExpression()
        {
            // The Pre-Order Traversal (L-ROOT-R) algorithm is used here 
            // Where Left (l) ==> Operands eg Numbers
            //       Root (ROOT) ==> Operator eg (*,+,-)
            //       Right (r) ==>  Operands eg Numbers
            var left = ParsePrimaryExpression();

            while (
            TokenIsPlusToken(Current) || TokenIsSubtractionToken(Current) ||
            TokenIsDivisionToken(Current) || TokenIsMultiplicationToken(Current)
            )
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }


        private SyntaxToken Match(SyntaxKind kind)
        {
            if (kind == Current.Kind) return NextToken();

            _diagnostics.Add($"ERROR: unexpected token '<{Current.Kind}>' expected '<{kind}>'");
            return new SyntaxToken(kind, Current.Position, null, null);
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

        private bool TokenIsMultiplicationToken(SyntaxToken token) => token.Kind == SyntaxKind.MultiplicationToken;
        private bool TokenIsDivisionToken(SyntaxToken token) => token.Kind == SyntaxKind.DivisionToken;
        private bool TokenIsSubtractionToken(SyntaxToken token) => token.Kind == SyntaxKind.SubtractionToken;
        private bool TokenIsPlusToken(SyntaxToken token) => token.Kind == SyntaxKind.PlusToken;
        private bool TokenIsWhiteSpace(SyntaxToken token) => token.Kind == SyntaxKind.WhiteSpaceToken;
        private bool TokenIsBadToken(SyntaxToken token) => token.Kind == SyntaxKind.BadToken;
    }

    class Evaluator
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
            if (node is NumberExpressionSyntax n)
                return (int)n.NumberToken.Value!;

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

            throw new Exception($"Unexpected node {node.Kind}");
        }
        
    }
}