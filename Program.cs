using compiler.CodeAnalysis;

namespace compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) break;

                var syntaxTree = SyntaxTree.Parse(line);
                
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.ResetColor();
            
                PrettyPrint(syntaxTree.Root);

                if (syntaxTree.Diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    foreach (var diagnostic in syntaxTree.Diagnostics) Console.WriteLine(diagnostic);
                }
                else
                {
                    var evaluator = new Evaluator(syntaxTree.Root);
                    var result = evaluator.Evaluate();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"result = {result}");
                }
            
                Console.ResetColor();
                
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

            indent += isLast ? "   " : "│   ";
            var lastChild = node.GetChildren().LastOrDefault();
            
            foreach (var child in node.GetChildren()) PrettyPrint(child, indent, child == lastChild);
        }
    }
    
}