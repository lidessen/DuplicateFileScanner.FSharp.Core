module CheckDuplicate

open System.IO
open System.Collections.Generic
open System
open System.Security.Cryptography
open System.Text

type DuplicateScanner() =
    let mutable fileDict = new Dictionary<string, string list>()
    let mutable count = 0
    let mutable files: string list = []
    
    member this.GetAllFiles(path: string):string list =
        let mutable result: string list = []
        if Directory.Exists(path) then
            Directory.GetFiles path
            |> Array.toList
            |> fun files ->
                result <- result @ files
                for dir in Directory.GetDirectories(path) do
                    result <- (this.GetAllFiles dir) @ result
        result
    
    member this.ProcessFile(file: string) =
        async {
            let md5Str = this.Md5ByFileName file
            count <- count + 1
            Console.Write (sprintf "\r已扫描[%d%%]" ((int)((float)count / (float)files.Length * 100.0)))
            if not (fileDict.ContainsKey md5Str) then
                fileDict.Add(md5Str, [file])
                ()
            else
                fileDict.[md5Str] <- (file :: fileDict.[md5Str])
                ()
        }
      
    member this.Md5ByFileName(fileName: string) =
        use stream = File.OpenRead(fileName)
        (this.Md5File stream)
    
    member _this.Md5File(fileStream: FileStream): string =
        use md5 = MD5.Create()
        (StringBuilder(), md5.ComputeHash(fileStream))
        ||> Array.fold (fun sb b -> sb.Append(b.ToString("x2")))
        |> string
    
    member this.Check(path: string) =
        fileDict <- new Dictionary<string, string list>()
        count <- 0
        let startTime = DateTime.Now
        if Directory.Exists(path) then
            Console.WriteLine ""
            this.GetAllFiles path
            |> fun t ->
                files <- t
                for file in files do
                    this.ProcessFile(file)
                    |> Async.RunSynchronously
                    |> ignore
                let endTime = DateTime.Now
                Console.Write (sprintf "\r扫描完成！ 耗时%f秒    \n\n" (endTime - startTime).TotalSeconds)
                fileDict
            |> fun dict ->
                for pair in dict do
                    if pair.Value.Length > 1 then
                        Console.WriteLine("{0}个文件重复", pair.Value.Length)
                        for file in pair.Value do
                            Console.WriteLine("     " + file)
                ()