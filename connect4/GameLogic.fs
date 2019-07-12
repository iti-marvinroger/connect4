module Connect4.GameLogic

open Connect4.Types

exception BadMoveException of string

let initializeBoard cols rowsPerCol : Board =
    Array.create cols (Array.create rowsPerCol None)

let private countColumns (board: Board) : int =
    Array.length board

let private countRows (board : Board) : int =
    Array.length (board.[0])

let private getColumn (board: Board) (x: int): BoardColumn = Array.get board x

let private getPawnAt (board: Board) ((x, y): Position): Pawn option = board.[x].[y]
let private setPawnAt (board: Board) ((x, y): Position) (pawn: Pawn): Board =
    let column = Array.copy (getColumn board x)
    Array.set column y (Some pawn)
    let board = Array.copy board
    Array.set board x column

    board

let private getMatchingAdjacentPawns (board: Board) (pawn: Pawn) (deltaX: int, deltaY: int) (start: Position): int =
    let columnsCount = countColumns board
    let rowsCount = countRows board
    let nextPosition ((x, y): Position) = (x + deltaX, y + deltaY)
    let inBoard ((x, y): Position) = 0 <= x && x < columnsCount && 0 <= y && y < rowsCount
    let samePlayer (position: Position) = 
        match getPawnAt board position with 
        | Some x -> x = pawn
        | None -> false
    let rec moveAlong acc pos = if inBoard pos && samePlayer pos then moveAlong (acc + 1) (nextPosition pos) else acc

    start |> moveAlong 0 |> fun res -> res - 1

let private checkIfMoveIsWinning (board: Board) (move: Position) (pawn: Pawn) =
    let checkIfLineIsWinning (deltaXA: int, deltaYA: int) (deltaXB: int, deltaYB: int) =
        let z (deltaX: int, deltaY: int) = getMatchingAdjacentPawns board pawn (deltaX, deltaY) move
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

let private canAddPawnToColumn (board: Board) (x: int) =
    let column = getColumn board x
    column.[0] = None

let addPawnToColumn (board: Board) (move: PlayMove) =
    match move.column < 0 || move.column > (countColumns board) - 1 with
    | true -> raise (BadMoveException "This column does not exist")
    | _ -> ()

    match canAddPawnToColumn board move.column with
    | true ->
        let column = getColumn board move.column
        let setPawnAndCheckWon (coord: Position) =
            let board = setPawnAt board coord move.pawn
            let won = checkIfMoveIsWinning board coord move.pawn

            { board = board; won = won }

        let rec traverse position =
            match position with
            | y when y = column.Length - 1 -> setPawnAndCheckWon (move.column, y)
            | _ ->
                match getPawnAt board (move.column, (position + 1)) with
                | None -> traverse (position + 1)
                | _ -> setPawnAndCheckWon (move.column, position)

        traverse 0
    | false -> raise (BadMoveException "This move is not possible")

// let addAndLog (column: int) (pawn: Pawn) (board: Board) =
//     let board, won = GameLogic.addPawnToColumn board column pawn
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