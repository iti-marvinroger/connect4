module Connect4.GameLogic

open Connect4.Types

exception BadMoveException of string

let private initializeBoard cols rowsPerCol : Board =
    Array.create cols (Array.create rowsPerCol None)

let initializeGame cols rows = { status = GameStatus.Ongoing; board = initializeBoard cols rows; lastPlayer = Pawn.None }

let private countColumns (board: Board) : int =
    Array.length board

let private countRows (board : Board) : int =
    Array.length (board.[0])

let private getColumn (board: Board) (x: int): BoardColumn = Array.get board x

let private isFull (board: Board) =
    let rec checkColumns columns = 
        match columns with
        | [] -> true
        | head :: tail ->
            match Array.contains None head with
            | true -> false
            | false -> checkColumns tail

    checkColumns (Array.toList board)

let private getPawnAt (board: Board) ((x, y): Position): Pawn = board.[x].[y]
let private setPawnAt (board: Board) ((x, y): Position) (pawn: Pawn): Board =
    let column = Array.copy (getColumn board x)
    Array.set column y pawn
    let board = Array.copy board
    Array.set board x column

    board

let private getMatchingAdjacentPawns (board: Board) (pawn: Pawn) (deltaX: int, deltaY: int) (start: Position): int =
    let columnsCount = countColumns board
    let rowsCount = countRows board
    let nextPosition ((x, y): Position) = (x + deltaX, y + deltaY)
    let inBoard ((x, y): Position) = 0 <= x && x < columnsCount && 0 <= y && y < rowsCount
    let samePlayer (position: Position) = getPawnAt board position = pawn 

    let rec moveAlong acc pos = if inBoard pos && samePlayer pos then moveAlong (acc + 1) (nextPosition pos) else acc

    start |> moveAlong 0 |> fun res -> res - 1

let private getStatusAfterMove (board: Board) (move: Position) (pawn: Pawn) =
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
       

    let isWin = checkCombinations [
        ((0, -1), (0, 1)); // column
        ((-1, 0), (1, 0)); // row
        ((-1, -1), (1, 1)); // diag1
        ((-1, 1), (1, -1)) // diag2
    ]

    let isBoardFull = isFull board

    match true with
    | x when x = isWin -> GameStatus.Won
    | x when x = isBoardFull -> GameStatus.Draw
    | _ -> GameStatus.Ongoing

let private canAddPawnToColumn (board: Board) (x: int) =
    let column = getColumn board x
    column.[0] = None

let addPawnToColumn (state: GameState) (move: PlayMove) =
    match move.column < 0 || move.column > (countColumns state.board) - 1 with
    | true -> raise (BadMoveException "This column does not exist")
    | _ -> ()

    match state.status = GameStatus.Ongoing with
    | false -> raise (BadMoveException "The game is over")
    | _ -> ()


    match state.lastPlayer = move.pawn with
    | true -> raise (BadMoveException "The player cannot play twice")
    | _ -> ()

    match canAddPawnToColumn state.board move.column with
    | true ->
        let column = getColumn state.board move.column
        let setPawnAndCheckWon (coord: Position) =
            let board = setPawnAt state.board coord move.pawn
            let status = getStatusAfterMove board coord move.pawn

            { board = board; status = status; lastPlayer = move.pawn }

        let rec traverse position =
            match position with
            | y when y = column.Length - 1 -> setPawnAndCheckWon (move.column, y)
            | _ ->
                match getPawnAt state.board (move.column, (position + 1)) with
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