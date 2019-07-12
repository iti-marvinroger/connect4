module Connect4.Types

type Position = int * int

type Pawn = 
    | Red
    | Yellow
type BoardCell = Option<Pawn>
type BoardColumn = array<BoardCell>
type Board = array<BoardColumn>

type GameState = {
    won: bool;
    board: Board;
}

type PlayMove = {
    pawn: Pawn;
    column: int;
}