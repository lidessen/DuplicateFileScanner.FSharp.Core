using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;

void Main(string[] args)
{
    var root = new RootCommand
    {
        new Argument<string>("dir").LegalFilePathsOnly(),
        new Option<string []>(new string[]{ "--ignore", "-i" }, (ArgumentResult result) => result.Tokens.Single().Value.Split(','), description: "ignore folders")
    };

    root.Handler = CommandHandler.Create<string, string[]>((dir, ignore) => {
        Console.WriteLine(dir);
        Console.WriteLine(ignore[0]);
    });

    root.Invoke(args);
}

Main(args);