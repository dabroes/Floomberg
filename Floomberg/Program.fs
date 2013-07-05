module Program

open System

let Wait() = 
    Console.WriteLine("Press a key")
    Console.ReadKey(true) |> ignore

[<EntryPoint>]
let main args =
    Console.WriteLine("HistoricalDataRequest")
    HistoricalDataRequest.RunExample()
    Wait()

    Console.WriteLine("IntradayBarDataRequest")
    IntradayBarDataRequest.RunExample()
    Wait()

    Console.WriteLine("IntraDayTickRequest")
    IntraDayTickRequest.RunExample()
    Wait()

    0