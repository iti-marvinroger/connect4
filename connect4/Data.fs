module Connect4.Data

open Connect4.Types

type State = {
  board: Board
}

module Data =
  let mutable private stateStorage = { board = GameLogic.initializeBoard 7 6 }

  let getState () = stateStorage
  let setState state =
    let newState = {
        board = state.board
    }
    stateStorage <- newState
    newState
