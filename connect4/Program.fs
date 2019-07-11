// Learn more about F# at http://fsharp.org

open connect4.Rest
open connect4.Data
open System
open Suave
open Suave.Successful

//[<EntryPoint>]
//let main argv =
//  startWebServer defaultConfig (OK "Hello, Suave!")
//  0

[<EntryPoint>]
let main argv =
  let personWebPart = rest "people" {
    GetAll = Data.sendResult
    Create = Data.getState
  }
  startWebServer defaultConfig personWebPart
  0




