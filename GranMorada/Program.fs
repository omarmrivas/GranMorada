open System
open System.Net
open System.Net.Sockets
open System.Threading
open Suave
open Suave.Http
open Suave.Filters
open Suave.Operators
open Suave.Successful


let getLocalIPAddress () = 
    let host = Dns.GetHostEntry(Dns.GetHostName())
    [for ip in host.AddressList do
        yield ip]
        |> List.find (fun ip -> ip.AddressFamily = AddressFamily.InterNetwork)
        |> (fun ip -> ip.ToString())

let execute exe =
    let proc = new System.Diagnostics.Process()
    let startInfo = new System.Diagnostics.ProcessStartInfo()
    startInfo.WindowStyle <- System.Diagnostics.ProcessWindowStyle.Hidden
    startInfo.FileName <- "/usr/bin/python"
    startInfo.Arguments <- exe
    proc.StartInfo <- startInfo
    proc.Start()

[<EntryPoint>]
let main argv = 
    let path = argv.[0]
    let ipaddress = getLocalIPAddress()
    printfn "Mounting server on: %s" ipaddress
    let cts = new CancellationTokenSource()
    let conf = { defaultConfig with cancellationToken = cts.Token
                                    bindings = [ HttpBinding.create HTTP IPAddress.Loopback 80us
                                                 HttpBinding.createSimple HTTP ipaddress 80
                                               ] }
    let valid (door, phone) =
        match door with
        | "enter" -> printfn "Activating entrance doors for phone %s" phone
                     let res = execute (path + "/activate1.sh")
                     printfn "Result: %A" res
                     OK "Activating entrance doors."
        | "exit" -> printfn "Activating exit doors for phone %s" phone
                    let res = execute (path + "/activate2.sh")
                    printfn "Result: %A" res
                    OK "Activating exit doors."
        | _ -> OK (sprintf "Error: %s - %s" door phone)
    let app = choose [
                GET >=> pathScan "/%s/%s" valid
              ]
    startWebServer conf app
//    let listening, server = startWebServerAsync conf app

(*    Async.Start(server, cts.Token)
    printfn "Make requests now"
    Console.ReadKey true |> ignore

    cts.Cancel()*)

    0    