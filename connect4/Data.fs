module Connect4.Data

let mutable private stateStorage = GameLogic.initializeGame 7 6

let getState = fun () -> stateStorage
let setState state =
    stateStorage <- state

    state
