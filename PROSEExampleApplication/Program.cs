using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Learning;
using System;
using Microsoft.ProgramSynthesis.VersionSpace;
using System.Collections.Generic;
using Microsoft.ProgramSynthesis.AST;

class Program
{
    static void Main(string[] args)
    {
        var compilationResult = DSLCompiler.LoadGrammarFromFile("SubstringExtraction.grammar");

        // With the following, we verify that we can indeed evaluate the DSL
        Console.WriteLine("===SEMANTICS TEST===");
        Console.WriteLine("Running test of the semantics engine");
        Console.WriteLine("The next line shoult read '=in='");
        var grammar = compilationResult.Value;
        var ast = grammar.ParseAST(
            "Substring(inp, PositionPair(RegexPosition(inp, RegexPair(/outside/, //), 0), RegexPosition(inp, RegexPair(//, /outside/), 1)))",
            ASTSerializationFormat.HumanReadable);
        var input = State.Create(grammar.InputSymbol, "outside=in=outside");
        var output = (string)ast.Invoke(input);
        Console.WriteLine(output);
        Console.WriteLine();

        // With the following, we verify that synthesis runs
        Console.WriteLine("===SYNTHESIS TEST===");
        Console.WriteLine("");
        Dictionary<State, object> examples;

        Console.WriteLine("Test 1");
        examples = new Dictionary<State, object>();
        input = State.Create(grammar.InputSymbol, "PROSE Rocks");
        examples[input] = "PROSE";
        testSynthesis(grammar, examples);

        Console.WriteLine("Test 2");
        examples = new Dictionary<State, object>();
        input = State.Create(grammar.InputSymbol, "Andrew0Head");
        examples[input] = "Head";
        input = State.Create(grammar.InputSymbol, "Codanda11Appachu");
        examples[input] = "Appachu";
        testSynthesis(grammar, examples);

        Console.WriteLine("Press any key to continue.");
        Console.ReadKey();
    }

    static void testSynthesis(Grammar grammar, Dictionary<State, object> examples)
    {
        var spec = new ExampleSpec(examples);
        var engine = new SynthesisEngine(grammar);
        ProgramSet learned = engine.LearnGrammar(spec);

        Console.WriteLine("Examples:");
        foreach (KeyValuePair<State, object> ex in examples)
            Console.WriteLine("(Input: " + ex.Key + ", Output: " + ex.Value + ")");

        Console.WriteLine("These are the programs that I learned");
        foreach (ProgramNode p in learned.RealizedPrograms)
        {
            Console.WriteLine(p);
            foreach (State inputExample in examples.Keys)
            {
                Console.WriteLine("Test: (Input: " + inputExample + ", Output: " + p.Invoke(inputExample) + ")");
            }
        }
        Console.WriteLine();

    }
}
