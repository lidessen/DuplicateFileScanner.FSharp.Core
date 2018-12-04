open System

type Color =
    | 红色 = 1
    | 绿色 = 2
    | 黄色 = 3

let test a b =
    (a,b)
[<EntryPoint>]
let main argv =
    Console.WriteLine Color.红色
    let (a, _) = test 1 2
    Console.WriteLine a
    Console.WriteLine (enum<Color> 3)
    0