open FindDuplicateFiles
open System

[<EntryPoint>]
let main argv =
    Console.WriteLine ""
    if argv.Length > 0 then FindRepeatFiles argv.[0]
    else FindRepeatFiles "."
    Console.Write "\r扫描完成！    \n\n"
    0