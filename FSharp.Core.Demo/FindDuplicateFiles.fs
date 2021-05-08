module FindDuplicateFiles

open System.Security.Cryptography
open System.Text
open System.IO
open System
open System.Collections.Generic
open System.Threading

type FileData =
    | Bytes of byte[]
    | Stream of FileStream

let Md5 (data: FileData): byte[] =
    use md5 = MD5.Create()
    match data with
    | Bytes(d) -> md5.ComputeHash d
    | Stream(d) -> md5.ComputeHash d

let Md5File (data: FileData): string =
    data
    |> Md5
    |> fun bytes -> (StringBuilder(), bytes)
    ||> Array.fold (fun sb b -> sb.Append(b.ToString("x2")))
    |> string

let Md5ByFileName (fileName: string): string option =
    try
        use stream = File.OpenRead(fileName)
        Some(Md5File (Stream(stream)))
    with _ -> None

let mutable fileDict = new Dictionary<string, string list>()
let mutable count = 0

let ConsolePadding (content: string) (length: int) : string =
    Array.create (Console.WindowWidth - content.Length) ' '
    |> String
    |> fun str -> content + str

type Printer() = 
    let lastLength = 0
    
    member this.Print (text: string) =
        if lastLength > 0 then
            lastLength |> ConsolePadding "\r" |> Console.Write |> ignore
        text |> Console.Write |> ignore
        lastLength = text.Length |> ignore
            

type FileCounter(printer: Printer) as this =
    member val Timer: Timer = new Timer((this.Print), null, 0, 100)
    member val Count = 0 with get, set
    member this.Print(_) =
        $"\rFind {this.Count} files" |> printer.Print |> ignore
    interface IDisposable with
        member _.Dispose() = this.Timer.Dispose()



let GetAllFiles (root: string) (ignores: string[]) : string list =
    use counter = new FileCounter(new Printer())
    let rec GetAllFilesRec (path: string) : string list =
        let mutable result: string list = []
        if Directory.Exists(path) then
            Directory.GetFiles path
            |> Array.toList
            |> fun files ->
                result <- result @ files
                counter.Count <- counter.Count + files.Length
                for dir in Directory.GetDirectories(path) do
                    let dirInfo = new DirectoryInfo(dir)
                    if not (Array.contains dirInfo.Name ignores) && not (Array.contains dirInfo.FullName ignores) then
                        result <- (GetAllFilesRec dir) @ result
        result
    GetAllFilesRec root

let ProcessFile (file: string, total: int) =
    async {
        try
            let md5Str = Md5ByFileName file
            count <- count + 1
            let percent = ((int)((float)count / (float)total * 100.0))
            Console.Write ($"\rScanning: [{percent}%%]")
            match md5Str with
            | Some(str) ->
                if not (fileDict.ContainsKey str) then
                    fileDict.Add(str, [file])
                    ()
                else
                    fileDict.[str] <- (file :: fileDict.[str])
                    ()
            | None -> ()
        with _ -> ()

    }

let FindRepeatFiles (path: string) (ignores: string[]) :unit =
    fileDict <- new Dictionary<string, string list>()
    count <- 0
    if Directory.Exists(path) then
        Console.WriteLine ""
        GetAllFiles path ignores
        |> fun files ->
            files
            |> List.map (fun file -> (ProcessFile(file, files.Length)))
            |> Async.Parallel
            |> Async.RunSynchronously
            |> ignore
            Console.Write "\rScan finished!    \n\n"
            fileDict
        |> fun dict ->
            for pair in dict do
                if pair.Value.Length > 1 then
                    Console.WriteLine("{0} files are same:", pair.Value.Length)
                    for file in pair.Value do
                        Console.WriteLine("     " + file)
            ()