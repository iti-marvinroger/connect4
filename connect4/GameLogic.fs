module GameLogic

exception BadMoveError of string

type Column = int
type Row = int
type Position = Column * Row

type Player = 
    | Red
    | Yellow
type BoardCell = Option<Player>
type BoardColumn = array<BoardCell>
type Board = array<BoardColumn>

type Status = 
    | Ongoing
    | Stuck
    | RedWon
    | YellowWon

type Game = {
    Board: Board;
    Status: Status
}

let initializeBoard cols rowsPerCol : Board =
    Array.create cols (Array.create rowsPerCol None)

let private countColumns (board: Board) : int =
    Array.length board

let private countRows (board : Board) : int =
    Array.length (board.[0])

let initializeGame board : Game = { Board = board; Status = Ongoing }

let private getColumn (board: Board) (x: int): BoardColumn = Array.get board x

let private getPieceAt (board: Board) ((x, y): Position): Player option = board.[x].[y]
let private setPieceAt (board: Board) ((x, y): Position) (player: Player): Board =
    let column = Array.copy (getColumn board x)
    Array.set column y (Some player)
    let board = Array.copy board
    Array.set board x column

    board

let private getMatchingAdjacentPieces (board: Board) (player: Player) (deltaX: int, deltaY: int) (start: Position): int =
    let columnsCount = countColumns board
    let rowsCount = countRows board
    let nextPosition ((x, y): Position) = (x + deltaX, y + deltaY)
    let inBoard ((x, y): Position) = 0 <= x && x < columnsCount && 0 <= y && y < rowsCount
    let samePlayer (position: Position) = 
        match getPieceAt board position with 
        | Some x -> x = player
        | None -> false
    let rec moveAlong acc pos = if inBoard pos && samePlayer pos then moveAlong (acc + 1) (nextPosition pos) else acc

    start |> moveAlong 0 |> fun res -> res - 1

let private checkIfMoveIsWinning (board: Board) (move: Position) (player: Player) =
    let checkIfLineIsWinning (deltaXA: int, deltaYA: int) (deltaXB: int, deltaYB: int) =
        let z (deltaX: int, deltaY: int) = getMatchingAdjacentPieces board player (deltaX, deltaY) move
        let a = z (deltaXA, deltaYA)
        let b = z (deltaXB, deltaYB)

        1 + a + b >= 4

    let rec checkCombinations combinations = 
        match combinations with
        | [] -> false
        | head :: tail ->
            let a, b = head
            match checkIfLineIsWinning a b with
            | true -> true
            | false -> checkCombinations tail

    checkCombinations [
        ((0, -1), (0, 1)); // column
        ((-1, 0), (1, 0)); // row
        ((-1, -1), (1, 1)); // diag1
        ((-1, 1), (1, -1)) // diag2
    ]

let private canAddPieceToColumn (board: Board) (x: int) =
    let column = getColumn board x
    column.[0] = None

let addPieceToColumn (board: Board) (columnX: int) (player: Player) =
    match columnX < 0 || columnX > (countColumns board) - 1 with
    | true -> raise (BadMoveError "This column does not exist")
    | _ -> ()

    match canAddPieceToColumn board columnX with
    | true ->
        let column = getColumn board columnX
        let setPieceAndCheckWon (coord: Position) =
            let board = setPieceAt board coord player
            let won = checkIfMoveIsWinning board coord player

            board, won

        let rec traverse position =
            match position with
            | y when y = column.Length - 1 -> setPieceAndCheckWon (columnX, y)
            | _ ->
                match getPieceAt board (columnX, (position + 1)) with
                | None -> traverse (position + 1)
                | _ -> setPieceAndCheckWon (columnX, position)

        traverse 0
    | false -> raise (BadMoveError "This move is not possible")

// let addAndLog (column: int) (player: Player) (board: Board) =
//     let board, won = GameLogic.addPieceToColumn board column player
//     match won with
//     | true -> printfn "Won"
//     | false -> printfn "-"

//     board

// printfn "Check column"
// GameLogic.initializeBoard 7 6
//     |> addAndLog 1 Red
//     |> addAndLog 1 Red
//     |> addAndLog 1 Red
//     |> addAndLog 1 Red
    
// printfn "Check row"
// GameLogic.initializeBoard 7 6
//     |> addAndLog 1 Red
//     |> addAndLog 2 Red
//     |> addAndLog 3 Red
//     |> addAndLog 4 Red

// printfn "Check diagonal"
// GameLogic.initializeBoard 7 6
//     |> addAndLog 1 Red
//     |> addAndLog 2 Yellow |> addAndLog 2 Red
//     |> addAndLog 3 Yellow |> addAndLog 3 Yellow |> addAndLog 3 Red
//     |> addAndLog 4 Yellow |> addAndLog 4 Yellow |> addAndLog 4 Yellow |> addAndLog 4 Red