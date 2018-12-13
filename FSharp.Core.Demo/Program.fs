open FindDuplicateFiles
open System

[<EntryPoint>]
let main argv =
    if argv.Length > 0 then FindRepeatFiles argv.[0]
    else FindRepeatFiles "."
    0