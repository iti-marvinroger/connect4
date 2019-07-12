module Connect4.Data

open Connect4.Types

module Data =
  let mutable private stateStorage = { won = false; board = GameLogic.initializeBoard 7 6 }

  let getState = stateStorage
  let setState state =
    stateStorage <- state

    state
