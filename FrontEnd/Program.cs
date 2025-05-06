using FrontEnd.Lexer;
using KukuLang.Interpreter.Interpreters.Main_Interpreter;
using KukuLang.Parser.Parsers.Recursive_MAIN_;

namespace KukuLang;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Enter the path to the source file: ");
        var sourcePath = Console.ReadLine();
        while (!File.Exists(sourcePath))
        {
            Console.WriteLine("File not found. Please enter a valid path:");
            sourcePath = Console.ReadLine();
        }

        string source = await File.ReadAllTextAsync(sourcePath);
        KukuLexer lexer = new(source);
        var tokens = lexer.Tokenize();
        tokens.ForEach(Console.WriteLine);

        var parser = new RecursiveDescentParser(tokens);
        var ast = parser.Parse(null);
        try
        {
            await ast.SaveMermaidAsPngAsync("ast-diagram.png");
            Console.WriteLine("PNG diagram saved to ast-diagram.png");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating PNG: {ex.Message}");
            Console.WriteLine("Mermaid code was saved as fallback.");
        }

        MainInterpreter interpreter = new(ast);
        interpreter.Interpret();
        //TODO: list
        //TODO: execute else block
        //TODO: import statements.
    }
}