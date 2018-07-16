open System
open System.Net
open System.Net.Sockets
open System.Threading
open Suave
open Suave.Http
open Suave.Filters
open Suave.Operators
open Suave.Successful
open System.IO
open DB

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
    // Create database
    DB.createDB()

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
        | "enter" -> 
            match selectUser (Some phone) with
            | [user] -> 
                if user.status = "1"
                then let res = execute (path + "/activate1.sh")
                     let response = sprintf "Activando portones de entrada para %s." user.name
                     OK response
                else let response = sprintf "Falta de pago de casa %s." user.house
                     OK response
            | _ -> let response = sprintf "Teléfono sin acceso: %s." phone
                   OK response
        | "exit" -> 
            match selectUser (Some phone) with
            | [user] -> 
                if user.status = "1"
                then let res = execute (path + "/activate2.sh")
                     let response = sprintf "Activando portones de salida para %s." user.name
                     OK response
                else let response = sprintf "Falta de pago de casa %s." user.house
                     OK response
            | _ -> let response = sprintf "Teléfono sin acceso: %s." phone
                   OK response
        | _ -> OK (sprintf "Error: %s - %s" door phone)

    let showUsers phone =
        selectUser phone
        |> List.map (WebUtility.HtmlEncode << string)
        |> String.concat "\n"

    let showHistory () =
        DB.selectHistory ()
        |> List.map (WebUtility.HtmlEncode << string)
        |> String.concat "\n"

    let newUser (phone, name, house, status) =
        addUser {
            phone = phone
            name = name
            house = house
            status = status
        } |> sprintf "Result: %i"

    let modifyUser (phone, name, house, status) =
        updateUser {
            phone = phone
            name = name
            house = house
            status = status
        } |> sprintf "Result: %i"

    let app = choose [
                GET >=> Suave.Filters.path "/admin/users" >=> (OK (showUsers None))
                GET >=> Suave.Filters.path "/admin/history" >=> (OK (showHistory ()))
                GET >=> pathScan "/%s/%s" valid
                GET >=> pathScan "/newuser/%s/%s/%s/%s" (fun user -> OK (newUser user))
                GET >=> pathScan "/modifyuser/%s/%s/%s/%s" (fun user -> OK (modifyUser user))
              ]
    startWebServer conf app
//    let listening, server = startWebServerAsync conf app

(*    Async.Start(server, cts.Token)
    printfn "Make requests now"
    Console.ReadKey true |> ignore

    cts.Cancel()*)

    0    