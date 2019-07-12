open Connect4

open System
open Suave
open Suave.Successful
open Connect4.Data

[<EntryPoint>]
let main argv =
  let boardWebPart = Rest.rest "board" {
    Get = fun () -> Data.getState
    Play = fun move ->
        let storedGameState = Data.getState

        let newGameState = GameLogic.addPawnToColumn storedGameState.board move

        Data.setState newGameState
  }

  startWebServer defaultConfig boardWebPart
  0
  