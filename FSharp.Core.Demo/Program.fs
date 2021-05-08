open CheckDuplicate
open System.CommandLine
open System.CommandLine.Invocation
open System
open System.CommandLine.Parsing

[<EntryPoint>]
let main argv =
    Console.OutputEncoding <- System.Text.Encoding.UTF8
    //let scanner = new DuplicateScanner()
    let rootCommand = new RootCommand()
    let arg = new Argument<string>("dir")
    let option = new Option<string[]>([|"--ignore"; "-i"|], fun (result: ArgumentResult) ->
        result.Tokens.[0].Value.Split(',')
        |> Array.map (fun path -> path.Trim())
    )
    rootCommand.AddOption option
    rootCommand.AddArgument (arg.LegalFilePathsOnly())
    rootCommand.Handler <- CommandHandler.Create<string, string[]>(
        fun dir ignore ->
            FindDuplicateFiles.FindRepeatFiles dir ignore
    )
    rootCommand.Invoke(argv) |> ignore
    0