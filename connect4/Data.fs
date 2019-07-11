namespace connect4.Data

type Pawn = 
| Red
| Yellow
| Void

type ListPawn =
| Empty
| Item of Pawn * ListPawn

type State = {
  CurrentPos: ListPawn
  Map: ListPawn
}

module Data =
  let mutable private stateStorage = Item(Red, Empty)
  let sendResult () = stateStorage
  let getState state =
    let newState = {
        CurrentPos = state.CurrentPos
        Map = state.Map
    }
    stateStorage <- newState.Map
    newState


