module Connect4.Types

type Position = int * int

type Pawn = 
    | Red
    | Yellow
type BoardCell = Option<Pawn>
type BoardColumn = array<BoardCell>
type Board = array<BoardColumn>

type GameStatus =
    | Ongoing
    | Won
    | Draw

type GameState = {
    status: GameStatus;
    board: Board;
    lastPlayer: Option<Pawn>
}

type PlayMove = {
    pawn: Pawn;
    column: int;
}