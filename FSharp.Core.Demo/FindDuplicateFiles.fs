module FindDuplicateFiles

open System.Security.Cryptography
open System.Text
open System.IO
open System
open System.Collections.Generic

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

let rec GetAllFiles (path: string): string list =
    let mutable result: string list = []
    if Directory.Exists(path) then
        Directory.GetFiles path
        |> Array.toList
        |> fun files ->
            result <- result @ files
            for dir in Directory.GetDirectories(path) do
                result <- (GetAllFiles dir) @ result
    result

let ProcessFile (file: string, total: int) =
    async {
        try
            let md5Str = Md5ByFileName file
            count <- count + 1
            let percent = ((int)((float)count / (float)total * 100.0))
            Console.Write ($"\r已扫描[{percent}%%]")
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

let FindRepeatFiles (path: string):unit =
    fileDict <- new Dictionary<string, string list>()
    count <- 0
    if Directory.Exists(path) then
        Console.WriteLine ""
        GetAllFiles path
        |> fun files ->
            files
            |> List.map (fun file -> (ProcessFile(file, files.Length)))
            |> Async.Parallel
            |> Async.RunSynchronously
            |> ignore
            Console.Write "\r扫描完成！    \n\n"
            fileDict
        |> fun dict ->
            for pair in dict do
                if pair.Value.Length > 1 then
                    Console.WriteLine("{0}个文件重复", pair.Value.Length)
                    for file in pair.Value do
                        Console.WriteLine("     " + file)
            ()