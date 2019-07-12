open Connect4.Rest
open Connect4.Data
open System
open Suave
open Suave.Successful

[<EntryPoint>]
let main argv =
  let boardWebPart = rest "board" {
    Get = Data.getState
    Play = Data.setState
  }

  startWebServer defaultConfig boardWebPart
  0
  