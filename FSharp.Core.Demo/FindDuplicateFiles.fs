module FindDuplicateFiles

open System.Security.Cryptography
open System.Text
open System.IO
open System
open System.Collections.Generic

let Md5 (data: byte[]): string =
    use md5 = MD5.Create()
    (StringBuilder(), md5.ComputeHash(data))
    ||> Array.fold (fun sb b -> sb.Append(b.ToString("x2")))
    |> string

let Md5File (fileStream: FileStream): string =
    use md5 = MD5.Create()
    (StringBuilder(), md5.ComputeHash(fileStream))
    ||> Array.fold (fun sb b -> sb.Append(b.ToString("x2")))
    |> string

let Md5ByFileName (fileName: string): string =
    use stream = File.OpenRead(fileName)
    (Md5File stream)

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
        let md5Str = Md5ByFileName file
        count <- count + 1
        Console.Write (sprintf "\r已扫描[%d%%]" ((int)((float)count / (float)total * 100.0)))
        if not (fileDict.ContainsKey md5Str) then
            fileDict.Add(md5Str, [file])
            ()
        else
            fileDict.[md5Str] <- (file :: fileDict.[md5Str])
            ()
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