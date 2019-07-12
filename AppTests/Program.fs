open NUnit.Framework
open Connect4

let getWonColumnGame = 
     GameLogic.initializeGame 7 6
          |> GameLogic.addPawnToColumn { column = 0; pawn = Types.Pawn.Red }
          |> GameLogic.addPawnToColumn { column = 1; pawn = Types.Pawn.Yellow }
          |> GameLogic.addPawnToColumn { column = 0; pawn = Types.Pawn.Red }
          |> GameLogic.addPawnToColumn { column = 1; pawn = Types.Pawn.Yellow }
          |> GameLogic.addPawnToColumn { column = 0; pawn = Types.Pawn.Red }
          |> GameLogic.addPawnToColumn { column = 1; pawn = Types.Pawn.Yellow }
          |> GameLogic.addPawnToColumn { column = 0; pawn = Types.Pawn.Red }

[<TestFixture>]
type TestClass() = 

    [<Test>]
    member this.CheckColumns() =  
        getWonColumnGame
          |> fun game -> Assert.AreEqual(game.status, Types.GameStatus.Won)

    [<Test>]
    member this.CheckRows() = 
        GameLogic.initializeGame 7 6
          |> GameLogic.addPawnToColumn { column = 0; pawn = Types.Pawn.Red }
          |> GameLogic.addPawnToColumn { column = 0; pawn = Types.Pawn.Yellow }
          |> GameLogic.addPawnToColumn { column = 1; pawn = Types.Pawn.Red }
          |> GameLogic.addPawnToColumn { column = 1; pawn = Types.Pawn.Yellow }
          |> GameLogic.addPawnToColumn { column = 2; pawn = Types.Pawn.Red }
          |> GameLogic.addPawnToColumn { column = 2; pawn = Types.Pawn.Yellow }
          |> GameLogic.addPawnToColumn { column = 3; pawn = Types.Pawn.Red }
          |> fun game -> Assert.AreEqual(game.status, Types.GameStatus.Won)

    [<Test>]
    member this.CheckDiagonals() = 
        GameLogic.initializeGame 7 6
          |> GameLogic.addPawnToColumn { column = 0; pawn = Types.Pawn.Red }
          |> GameLogic.addPawnToColumn { column = 1; pawn = Types.Pawn.Yellow }
          |> GameLogic.addPawnToColumn { column = 1; pawn = Types.Pawn.Red }
          |> GameLogic.addPawnToColumn { column = 6; pawn = Types.Pawn.Yellow }
          |> GameLogic.addPawnToColumn { column = 2; pawn = Types.Pawn.Red }
          |> GameLogic.addPawnToColumn { column = 2; pawn = Types.Pawn.Yellow }
          |> GameLogic.addPawnToColumn { column = 2; pawn = Types.Pawn.Red }
          |> GameLogic.addPawnToColumn { column = 3; pawn = Types.Pawn.Yellow }
          |> GameLogic.addPawnToColumn { column = 3; pawn = Types.Pawn.Red }
          |> GameLogic.addPawnToColumn { column = 3; pawn = Types.Pawn.Yellow }
          |> GameLogic.addPawnToColumn { column = 3; pawn = Types.Pawn.Red }
          |> fun game -> Assert.AreEqual(game.status, Types.GameStatus.Won)

    [<Test>]
    member this.CheckCantPlayWhenWon() =  
        getWonColumnGame
          |> fun game -> Assert.Throws<GameLogic.BadMoveException>(fun () -> GameLogic.addPawnToColumn { column = 0; pawn = Types.Pawn.Red } |> ignore) |> ignore