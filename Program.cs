﻿using compiler.CodeAnalysis;
using compiler.CodeAnalysis.Syntax;

namespace compiler
{
    static class Program
    {
        static void Main()
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) break;

                var syntaxTree = SyntaxTree.Parse(line);
                
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.ResetColor();
            
                PrettyPrint(syntaxTree.Root);

                // Diagnostics means errors 
                // if there is any error
                if (syntaxTree.Diagnostics.Any())
                {
                    // change the console to red
                    Console.ForegroundColor = ConsoleColor.Red;

                    // print each diagnostic
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