open CheckDuplicate
open System.CommandLine
open System.CommandLine.Invocation
open System

[<EntryPoint>]
let main argv =
    Console.OutputEncoding <- System.Text.Encoding.UTF8
    //let scanner = new DuplicateScanner()
    let rootCommand = new RootCommand()
    let arg = new Argument<string>("dir")
    arg.SetDefaultValue "."
    rootCommand.AddArgument arg
    rootCommand.Handler <- CommandHandler.Create<string>(fun (dir: string) -> FindDuplicateFiles.FindRepeatFiles dir)
    rootCommand.Invoke(argv) |> ignore
    0