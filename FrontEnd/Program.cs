using FrontEnd.Lexer;
using KukuLang.Interpreter.Interpreters.Main_Interpreter;
using KukuLang.Parser.Parsers.Recursive_MAIN_;

namespace KukuLang;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter the path to the source file: ");
        var sourcePath = Console.ReadLine();
        while (!File.Exists(sourcePath))
        {
            Console.WriteLine("File not found. Please enter a valid path:");
            sourcePath = Console.ReadLine();
        }

        string source = File.ReadAllText(sourcePath);
        KukuLexer lexer = new(source);
        var tokens = lexer.Tokenize();
        tokens.ForEach(Console.WriteLine);

        var parser = new RecursiveDescentParser(tokens);
        var ast = parser.Parse(null);
        Console.WriteLine(ast.ToString(0));

        MainInterpreter interpreter = new(ast);
        interpreter.Interpret();
        //TODO: list
        //TODO: execute else block
        //TODO: import statements.
    }
}