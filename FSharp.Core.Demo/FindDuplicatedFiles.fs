module FindDuplicatedFiles

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

let FindRepeatFiles (path: string):unit =
    Console.WriteLine ""
    let fileDict = new Dictionary<string, string list>()
    if Directory.Exists(path) then
        Directory.GetFiles(path)
        |> Array.toList
        |> fun files ->
            let mutable count = 0
            for file in files do
                count <- count + 1
                Console.Write (sprintf "\r已扫描[%d%%]" ((int)((float)count / (float)files.Length * 100.0)))
                let md5Str = Md5ByFileName file
                if not (fileDict.ContainsKey md5Str) then
                    fileDict.Add(md5Str, [file])
                    ()
                else
                    fileDict.[md5Str] <- (file :: fileDict.[md5Str])
                    ()
            Console.Write "\r扫描完成！    \n\n"
            fileDict
        |> fun dict ->
            for pair in dict do
                if pair.Value.Length > 1 then
                    Console.WriteLine("{0}个文件重复", pair.Value.Length)
                    for file in pair.Value do
                        Console.WriteLine("     " + file)
            ()