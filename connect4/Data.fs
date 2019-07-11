namespace connect4.Data

type Pawn = 
| Red
| Yellow

type BoardCell = Option<Pawn>
type BoardColumn = array<BoardCell>
type Board = array<BoardColumn>

type Pos = {
 X : int
 Y : int
}


type State = {
  LastPawn: Pos
  Map: Board
}

module Data =
  let mutable private stateStorage = { LastPawn = { X = 0; Y = 0 }; Map = (Array.create 7 (Array.create 6 None))}
  let sendResult () = stateStorage
  let getState state =
    let newState = {
        LastPawn = state.LastPawn
        Map = state.Map
    }
    stateStorage <- newState
    newState