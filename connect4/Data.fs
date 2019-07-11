namespace connect4.Data

open System.Collections.Generic

type Pawn = 
| Red
| Yellow
| Void

type ListPawn =
| Empty
| Item of Pawn * ListPawn

type State = {
  Map: ListPawn
}

module Data =

  let mutable private stateStorage = ListPawn(Empty)
  let sendResult () =
    stateStorage |> Seq.map (fun p -> p)

  let getState state =
    let newState = {
        Map = state.Map
    }
    stateStorage <- newState.Map
    newState


