import Browser
import Html exposing (..)
import Http
import Json.Decode exposing (Decoder, field, string, at, map, map3, succeed)
import Json.Encode as Encode

-- MAIN
main =
  Browser.element
    { init = init
    , update = update
    , subscriptions = subscriptions
    , view = view
    }

-- MODEL
type Model
  = Ongoing UpdatedBoard
  | Finish

-- type alias Case = 
--   {
--     case : String
--   }

type Square
  = Red
  | Yellow

type alias Post = 
  { column : Int
  , square : String
  }

type alias UpdatedBoard =
  { status : String
  , board : List (List Post)
  , square : Square
  }


init : () -> (Model, Cmd Msg)
init _ =
  ( Ongoing { status = "Ongoing", board = [], square = Red }
  , postPawn { column = 1, square = "Red" }
  )


postPawn : Post -> Cmd Msg
postPawn post =
    createPawnRequest post


createPawnRequest : Post -> Cmd Msg
createPawnRequest post =
  Http.request
    { method = "POST"
    , headers =
        [ Http.header "Origin" "http://localhost:8080"
        , Http.header "Access-Control-Request-Method" "POST"
        , Http.header "Access-Control-Request-Headers" "X-Custom-Header"
        ]
    , url = "http://localhost:8080/board/"
    , body = Http.jsonBody (postEncoder post)
    , expect = Http.expectJson GotBoard postDecoder
    , timeout = Nothing
    , tracker = Nothing
    }


postEncoder : Post -> Encode.Value
postEncoder post =
    Encode.object
      [ ( "pawn"
        , Encode.object
            [ ("case", Encode.string post.square)
            ]
        )
      , ( "column", Encode.int post.column )
      ]

postDecoder : Decoder UpdatedBoard
postDecoder =
  map3 UpdatedBoard
    (field "status" statusDecoder)
    (field "board" boardDecoder)
    (field "square" squareDecoder)


statusDecoder : Decoder String
statusDecoder = field "case" string

boardDecoder : Decoder (List (List Post))
boardDecoder = succeed []

squareDecoder : Decoder Square
squareDecoder = succeed Red

-- UPDATE
type Msg
  = GotBoard (Result Http.Error UpdatedBoard)

update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
  case msg of
    GotBoard result ->
      case result of
        Ok board ->
          (Ongoing board, Cmd.none)

        Err _ ->
          (model, Cmd.none)


-- SUBSCRIPTIONS
subscriptions : Model -> Sub Msg
subscriptions model =
  Sub.none

-- VIEW
view : Model -> Html Msg
view model =
  case model of
    Ongoing board ->
      text "Displays board here."

    Finish ->
      text "Finish..."
