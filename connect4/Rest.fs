module Connect4.Rest

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Suave.Successful
open Suave.Filters
open Suave.Writers

type RestError = {
    error: string
}

type RestActions = {
  Get: unit -> Types.GameState
  Play: Types.PlayMove -> Types.GameState
  Reset: unit -> Types.GameState
}

let JSON v =
  let jsonSerializerSettings = new JsonSerializerSettings()
  jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

  JsonConvert.SerializeObject(v, jsonSerializerSettings)
  |> OK
  >=> Writers.setMimeType "application/json; charset=utf-8"

let fromJson<'a> json = JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

let getResourceFromReq<'a> (req : HttpRequest) =
  let getString rawForm =
    System.Text.Encoding.UTF8.GetString(rawForm)
  req.rawForm |> getString |> fromJson<'a>

let rest resource =
  let handleGet = warbler (fun _ -> resource.Get () |> JSON)
  let handleDelete = warbler (fun _ -> resource.Reset () |> JSON)
  let handlePost = request (fun r ->
    try
        let move = getResourceFromReq<Types.PlayMove> r
        let result = resource.Play move
        JSON result
    with _ -> JSON { error = "Bad request" }
  )

  let setCORSHeaders =
    addHeader  "Access-Control-Allow-Origin" "*" 
    >=> addHeader "Access-Control-Allow-Headers" "content-type" 
    >=> addHeader "Access-Control-Allow-Methods" "GET,OPTIONS,POST,DELETE" 

  let allowCors : WebPart = fun context ->
    context |> (
        setCORSHeaders )

  path "/board" >=> choose [
    GET >=> allowCors >=> handleGet
    POST >=> allowCors >=> handlePost
    DELETE >=> allowCors >=> handleDelete
    OPTIONS >=> allowCors >=> JSON { error = "For CORS" }
  ]



