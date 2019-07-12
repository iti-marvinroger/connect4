module Connect4.Types

open Newtonsoft.Json
open Newtonsoft.Json.FSharp.Idiomatic

type Position = int * int

[<JsonConverter(typeof<SingleCaseDuConverter>)>]
type Pawn = 
    | Red
    | Yellow
    | None
type BoardCell = Pawn
type BoardColumn = array<BoardCell>
type Board = array<BoardColumn>

[<JsonConverter(typeof<SingleCaseDuConverter>)>]
type GameStatus =
    | Ongoing
    | Won
    | Draw

type GameState = {
    status: GameStatus;
    board: Board;
    lastPlayer: Pawn
}

type PlayMove = {
    pawn: Pawn;
    column: int;
}