module Connect4.Rest

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Suave.Successful
open Suave.Filters

type RestActions = {
  Get: unit -> Types.GameState
  Play: Types.PlayMove -> Types.GameState
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

let rest resourceName resource =
  let resourcePath = "/" + resourceName
  let get = warbler (fun _ -> resource.Get () |> JSON)

  path resourcePath >=> choose [
    GET >=> get
    POST >=> request (getResourceFromReq<Types.PlayMove> >> resource.Play >> JSON)
  ]



