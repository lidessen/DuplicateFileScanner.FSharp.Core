open CheckDuplicate

[<EntryPoint>]
let main argv =
    let scanner = new DuplicateScanner()
    if argv.Length > 0 then scanner.Check argv.[0]
    else scanner.Check "."
    0