﻿namespace Newtonsoft.Json.FSharp.Idiomatic

open System
open FSharp.Reflection

open Newtonsoft.Json

/// F# options-converter
type OptionConverter() =
  inherit JsonConverter()
  let optionTy = typedefof<option<_>>

  override __.CanConvert t =
    t.IsGenericType
    && optionTy.Equals (t.GetGenericTypeDefinition())

  override __.WriteJson(writer, value, serializer) =
    let value =
      if isNull value then
        null
      else
        let _,fields = FSharpValue.GetUnionFields(value, value.GetType())
        fields.[0]
    serializer.Serialize(writer, value)

  override __.ReadJson(reader, t, _existingValue, serializer) =
    let innerType = t.GetGenericArguments().[0]

    let innerType =
      if innerType.IsValueType then
        typedefof<Nullable<_>>.MakeGenericType([| innerType |])
      else
        innerType

    let value = serializer.Deserialize(reader, innerType)
    let cases = FSharpType.GetUnionCases t

    if isNull value then
      FSharpValue.MakeUnion(cases.[0], [||])
    else
      FSharpValue.MakeUnion(cases.[1], [|value|])

module Reflection =

  // swap these commented lines to get logging
  let logfn fmt =
    Printf.kprintf ignore fmt
    //printfn fmt

  open Newtonsoft.Json.Linq

  let allCasesEmpty (y: System.Type) = y |> FSharpType.GetUnionCases |> Array.forall (fun case -> case.GetFields() |> Array.isEmpty)
  let isList (y: System.Type) = y.IsGenericType && typedefof<List<_>> = y.GetGenericTypeDefinition ()
  let isOption (y: System.Type) = y.IsGenericType && typedefof<_ option> = y.GetGenericTypeDefinition ()

  let (|MapKey|_|) (key: 'a) map = map |> Map.tryFind key

  let advance (reader: JsonReader) =
    reader.Read() |> ignore
    reader

  let tokToReader (tok: #JToken) = tok.CreateReader ()

  let rec readValueIntoJToken   (reader: JsonReader): JToken * JsonReader =
      logfn "reading token of type %s" (string reader.TokenType)
      let result =
        match reader.TokenType with
        | JsonToken.Boolean     -> (reader.Value :?> bool)    |> JToken.op_Implicit
        | JsonToken.Float       -> (reader.Value :?> float)   |> JToken.op_Implicit
        | JsonToken.Integer     -> (reader.Value :?> int64)   |> JToken.op_Implicit
        | JsonToken.String      -> (reader.Value :?> string)  |> JToken.op_Implicit
        | JsonToken.Null        -> JValue.CreateNull()        :> JToken
        | x                     -> failwithf "don't know how to read value of type %s into a JToken" (string x)
      result, advance reader
  and readObjectIntoJObject     (reader: JsonReader): JObject * JsonReader =
    let rec loop (reader: JsonReader) (o: JObject): JObject * JsonReader =
      match reader.TokenType with
      | JsonToken.EndObject ->
        o, advance reader
      | JsonToken.PropertyName ->
        let name = reader.Value :?> string
        let value, reader' = readPropertyIntoJToken (advance reader)
        o.[name] <- value
        loop reader' o
      | x -> failwithf "wasn't expecting token of type %s in object" (string x)
    logfn "about to read object, token is of type %s" (string reader.TokenType)
    loop (advance reader) (JObject())
  and readArrayIntoJArray       (reader: JsonReader): JArray * JsonReader  =
    let reader' = advance reader
    // what kind of items are we pulling out?
    let readF =
      match reader'.TokenType with
      | JsonToken.Boolean
      | JsonToken.Float
      | JsonToken.Integer
      | JsonToken.String ->
        logfn "reading array of %ss" (string reader'.TokenType)
        readValueIntoJToken
        // read many values until end of array
      | JsonToken.StartArray ->
        logfn "reading array of arrays"
        readArrayIntoJArray >> fun (a, reader) -> a :> _, reader
      | JsonToken.StartObject ->
        logfn "reading array of objects"
        readObjectIntoJObject >> fun (o, reader) -> o :> _, reader
      | n -> failwithf "don't know how to read multiples of %s" (string n)

    let rec loop (reader: JsonReader) (arr: JArray) =
      match reader.TokenType with
      | JsonToken.EndArray ->
        arr, advance reader
      | _ ->
        let item, reader' = readF reader
        logfn "next token is of type %s" (string reader'.TokenType)
        arr.Add item
        loop reader' arr

    loop reader (JArray())

  and readPropertyIntoJToken (reader: JsonReader): JToken * JsonReader =
    match reader.TokenType with
    | JsonToken.Boolean
    | JsonToken.Float
    | JsonToken.Integer
    | JsonToken.String
    | JsonToken.Null        -> readValueIntoJToken reader
    | JsonToken.StartArray  -> readArrayIntoJArray reader |> fun (tok, reader) -> tok :> _ , reader
    | JsonToken.StartObject -> readObjectIntoJObject reader |> fun (tok, reader) -> tok :> _, reader
    | x                     -> failwithf "value reader doesn't know how to handle JToken of type %s" (string x)

  /// needs to read a jsonreader's properties and buffer the various json property values into separate JsonReaders,
  /// so that we can use the property names later to find the case info containing those properties,
  /// so that we can use the case reflection info to actually deserialize the jsonreaders into the correct types
  let getPropsAndReaders (reader: JsonReader) : Map<string, JsonReader> =
    let rec loop (reader: JsonReader) propMap =
      match reader.TokenType with
      | JsonToken.None -> propMap
      | JsonToken.EndObject ->
        loop (advance reader) propMap
      | JsonToken.PropertyName ->
        let name = reader.Value :?> string
        logfn "reading '%s'" name
        let reader' = advance reader
        let (property, reader'') = readPropertyIntoJToken reader'
        loop (reader'') (propMap |> Map.add name (tokToReader property))
      | x -> failwithf "outer property reader loop doesn't know how to handle JToken of type %s" (string x)

    loop (advance reader) Map.empty

open Reflection

/// A converter that seamlessly converts enum-style discriminated unions, that is unions where every case has no data attached to it
type SingleCaseDuConverter () =
  inherit JsonConverter ()

  override __.CanConvert y =
    FSharpType.IsUnion y && allCasesEmpty y

  override __.WriteJson(writer, value, serializer) =
    // writing a single-case union is just the 'name' of the case
    let case, _ = FSharpValue.GetUnionFields(value, value.GetType())
    serializer.Serialize(writer, case.Name)

  /// reading a single-case union is just constructing a new instance of that particular case
  override __.ReadJson(reader, t, _existingValue, _serializer) =
    logfn "type name is %s" t.FullName
    logfn "token type is %s" (string reader.TokenType)
    let caseName = string reader.Value
    logfn "Case name is %s" caseName
    let case = FSharpType.GetUnionCases t |> Array.tryFind (fun c -> c.Name.Equals(caseName, StringComparison.OrdinalIgnoreCase))
    match case with
    | Some case ->
      FSharpValue.MakeUnion(case, [||])
    | None ->
      failwithf "Unknown union case %s for Union Type %s" caseName t.FullName

/// a serializer that will handle DUs with fields, as long as the kind comes first.
type MultiCaseDuConverter (casePropertyName) =
  inherit JsonConverter ()

  new () = MultiCaseDuConverter("kind")

  override __.CanConvert y =
    FSharpType.IsUnion y
    && not (allCasesEmpty y)
    && not (isList y)
    && not (isOption y)

  override __.WriteJson(writer, value, serializer) =
    /// writes an output object whose properties are { "kind": CASE_NAME, "case_prop1": case_value_1 }, etc.
    let case, fields = FSharpValue.GetUnionFields(value, value.GetType())
    let propInfos = case.GetFields() |> Array.map (fun propInfo -> propInfo.Name);
    let zippedProperties = Array.zip fields propInfos

    writer.WriteStartObject ();
    // write case name
    writer.WritePropertyName casePropertyName
    writer.WriteValue case.Name

    // write properties
    for (field, name) in zippedProperties do
      writer.WritePropertyName name
      serializer.Serialize(writer, field)

    writer.WriteEndObject()

  override __.ReadJson(reader, t, _existingValue, serializer) =
    let cases = FSharpType.GetUnionCases t
    let rec loop (reader: JsonReader) caseAndFields propValues =
      logfn "tokentype %s" (string reader.TokenType)
      logfn "value %s" (string reader.Value)
      // should start at StartObject
      match reader.TokenType, caseAndFields with
      | JsonToken.StartObject, _
      | JsonToken.EndObject, None ->
        reader.Read () |> ignore
        loop reader caseAndFields propValues
      | JsonToken.PropertyName, None when string reader.Value = casePropertyName ->
        let caseName = reader.ReadAsString()
        match cases |> Seq.tryFind (fun c -> c.Name = caseName) with
        | Some case ->
          let fields = case.GetFields()
          reader.Read () |> ignore
          loop reader (Some (case, fields)) Map.empty
        | None ->
          failwithf "unknown case %s for type %s" caseName t.FullName
      | JsonToken.PropertyName, None ->
        // unordered fields :(
        failwithf "unordered field %s" (string reader.Value)
      | JsonToken.PropertyName, (Some (_, fields) as caseAndFields) ->
        let fieldName = string reader.Value
        match fields |> Array.tryFind (fun field -> field.Name = fieldName) with
        | Some field ->
          reader.Read () |> ignore
          let fieldValue = serializer.Deserialize(reader, field.PropertyType)
          reader.Read () |> ignore
          loop reader caseAndFields (propValues |> Map.add fieldName fieldValue)
        | None ->
          failwithf "unknown field %s" (string reader.Value)
      | JsonToken.EndObject, Some (case, fields) ->
        let fieldsInOrder = fields |> Array.map (fun f -> Map.find f.Name propValues)
        FSharpValue.MakeUnion(case, fieldsInOrder)
      | x ->
        failwithf "unhandled token type %s" (string x)
    loop reader None Map.empty

/// a serializer that looks at the propertyNames of fields as they come in and attempts to serialize the fields as soon as there is a distinguishing property name
type OutOfOrderMultiCaseDuConverter () =
  inherit JsonConverter ()

  static let isExactMatch caseFields providedFields = caseFields = providedFields

  override __.CanConvert y =
    FSharpType.IsUnion y
    && not (Reflection.allCasesEmpty y)
    && not (Reflection.isList y)
    && not (Reflection.isOption y)

  override __.WriteJson (writer, value, serializer) =
    // writes an output object whose properties are the fields of the DU.
    let case, fields = FSharpValue.GetUnionFields(value, value.GetType())
    let propInfos = case.GetFields() |> Array.map (fun propInfo -> propInfo.Name);
    let zippedProperties = Array.zip fields propInfos

    writer.WriteStartObject ();
    for (field, name) in zippedProperties do
      writer.WritePropertyName name
      serializer.Serialize(writer, field)
    writer.WriteEndObject()

  override __.ReadJson (reader, t, _existingValue, serializer) =
    /// while reading, we'll look at a property name and see if it's distinct. if it is, then great!
    let cases = FSharpType.GetUnionCases t
    let casesByName = cases |> Array.map (fun case -> case.Name, case) |> Map.ofArray
    let fieldsByCaseName = cases |> Array.map (fun case -> case.Name, case.GetFields()) |> Map.ofArray
    let fieldNamesByCase = fieldsByCaseName |> Map.map (fun _case fields  -> fields |> Seq.map (fun field -> field.Name) |> Set.ofSeq)

    let isSubsetAndAllMissingAreOptional ((caseName, caseFields): string * Set<string>) (providedFields: Set<string>) =
      match caseFields.IsSupersetOf providedFields with
      | true ->
        let caseProps = fieldsByCaseName |> Map.find caseName
        let missingFields = (caseFields - providedFields)
        let missingFieldProps =
          missingFields
          |> Seq.map (fun missingFieldName -> caseProps |> Array.tryFind (fun prop -> prop.Name = missingFieldName))
        missingFieldProps
        |> Seq.forall (function | None -> false | Some p -> isOption p.PropertyType)
      | false -> false

    let findCaseByProperties allProps =
      let propSet = allProps |> Set.ofSeq
      match fieldNamesByCase |> Map.filter (fun key fields -> isExactMatch fields propSet || isSubsetAndAllMissingAreOptional (key, fields) propSet) |> Map.toList with
      | [] -> failwithf "no case of type `%s` has properties named %A" t.AssemblyQualifiedName propSet
      | [(case, _fields)] -> casesByName |> Map.find case
      | cases -> failwithf "mutiple cases (%A) of type `%s` have properties named %A" (cases |> List.map fst) t.AssemblyQualifiedName propSet

    let propsAndReaders = getPropsAndReaders reader
    let propNames = propsAndReaders |> Map.toSeq |> Seq.map fst
    logfn "looking for case with properties %A" propNames
    let case = findCaseByProperties propNames
    logfn "found case %s" case.Name
    let caseFields = fieldsByCaseName |> Map.find case.Name
    let casePropsInOrder =
      caseFields
      |> Array.map (fun prop ->
        let existingReader = propsAndReaders |> Map.tryFind prop.Name
        match existingReader with
        | Some reader -> serializer.Deserialize(reader, prop.PropertyType)
        // this line makes it so that if a json object is missing a field and the backing field is an 'a Option, `None` is used instead.
        | None when isOption prop.PropertyType -> box None
        | None -> failwithf "no value provided for required property %s of DU case %s" prop.Name case.Name
      )
    FSharpValue.MakeUnion(case, casePropsInOrder)
