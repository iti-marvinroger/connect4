open Connect4

open System
open Suave

open Connect4.Rest

[<EntryPoint>]
let main argv =
  let boardWebPart = Rest.rest {
    Get = fun () -> Data.getState ()
    Play = fun move ->
        let storedGameState = Data.getState ()
        let newGameState = GameLogic.addPawnToColumn move storedGameState

        Data.setState newGameState
    Reset = fun () -> Data.setState (GameLogic.initializeGame 7 6)
  }

  startWebServer defaultConfig boardWebPart
  0
  