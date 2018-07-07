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
    let ipaddress = getLocalIPAddress()
    printfn "Mounting server on: %s" ipaddress
    let cts = new CancellationTokenSource()
    let conf = { defaultConfig with cancellationToken = cts.Token
                                    bindings = [ HttpBinding.create HTTP IPAddress.Loopback 80us
                                                 HttpBinding.createSimple HTTP ipaddress 80
                                               ] }
    let valid str =
        match str with
        | "1" -> printfn "Activating 1"
                 let res = execute "activate1.sh"
                 printfn "Result: %A" res
                 OK str
        | "2" -> printfn "Activating 2"
                 let res = execute "activate2.sh"
                 printfn "Result: %A" res
                 OK str
        | _ -> OK (sprintf "Error: %s" str)
    let app = choose [
                GET >=> pathScan "/%s" valid
              ]
    let listening, server = startWebServerAsync conf app

    Async.Start(server, cts.Token)
    printfn "Make requests now"
    Console.ReadKey true |> ignore

    cts.Cancel()

    0    