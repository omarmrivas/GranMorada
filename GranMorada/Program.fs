open System
open System.Threading
open Suave

[<EntryPoint>]
let main argv = 
  let cts = new CancellationTokenSource()
  let conf = { defaultConfig with cancellationToken = cts.Token
                                  bindings = [ HttpBinding.createSimple HTTP "10.0.1.4" 8082] }
  let listening, server = startWebServerAsync conf (Successful.OK "Hello World")
    
  Async.Start(server, cts.Token)
  printfn "Make requests now"
  Console.ReadKey true |> ignore
    
  cts.Cancel()

  0    