import Browser
import Html exposing (..)
import Html.Attributes exposing (..)
import Html.Events exposing (onClick)

-- INTERNAL MODULES

-- import Styles exposing (..)

main =
  Browser.sandbox { init = init, update = update, view = view }


-- MODEL

type alias Model = 
  {
    buttons : List Int,
    board : List (List CaseState)
  }

init : Model
init =
  Model
  [1, 2, 3, 4, 5, 6, 7] 
  [
    [Empty, Empty, Empty, Empty, Empty, Empty, Empty],
    [Empty, Empty, Empty, Empty, Empty, Empty, Empty],
    [Empty, Empty, Empty, Empty, Empty, Empty, Empty],
    [Empty, Empty, Empty, Empty, Empty, Empty, Empty],
    [Empty, Empty, Empty, Empty, Empty, Empty, Empty],
    [Empty, Empty, Empty, Empty, Empty, Empty, Empty]
  ]

-- UPDATE

type Msg = Increment

type CaseState 
  = Empty
  | PlayerOne
  | PlayerTwo

update : Msg -> Model -> Model
update msg model =
  case msg of
    Increment ->
      model


-- VIEW   

view : Model -> Html Msg
view model =
  div [ ]
    [ div
        [ ]
        (List.map(\i -> button [] [ text "button" ]) model.buttons)
    , div 
        [ class "gridContainer"
        , style "display" "flex"
        ]
        (List.map (\l -> div 
                            [class "column"]
                            (List.map(\n -> div
                                              [class "square"]
                                              [text "ok"]) l)) model.board)
    ]