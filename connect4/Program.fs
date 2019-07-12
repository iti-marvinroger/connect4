open Connect4

open System
open Suave

open Connect4.Rest

[<EntryPoint>]
let main argv =
  let boardWebPart = Rest.rest "board" {
    Get = fun () -> Data.getState ()
    Play = fun move ->
        let storedGameState = Data.getState ()
        let newGameState = GameLogic.addPawnToColumn storedGameState move

        Data.setState newGameState
  }

  startWebServer defaultConfig boardWebPart
  0
  