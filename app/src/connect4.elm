import Browser
import Array
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
    board : List (List Case)
  }

init : Model
init =
  Model
  [1, 2, 3, 4, 5, 6, 7] 
  [
    [{ state = Empty, id =  2 }, { state = PlayerOne, id =  2 }, { state = PlayerTwo, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }],
    [{ state = Empty, id =  2 }, { state = PlayerOne, id =  2 }, { state = PlayerTwo, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }],
    [{ state = Empty, id =  2 }, { state = PlayerOne, id =  2 }, { state = PlayerTwo, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }],
    [{ state = Empty, id =  2 }, { state = PlayerOne, id =  2 }, { state = PlayerTwo, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }],
    [{ state = Empty, id =  2 }, { state = PlayerOne, id =  2 }, { state = PlayerTwo, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }],
    [{ state = Empty, id =  2 }, { state = PlayerOne, id =  2 }, { state = PlayerTwo, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }],
    [{ state = Empty, id =  2 }, { state = PlayerOne, id =  2 }, { state = PlayerTwo, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }, { state = Empty, id =  2 }]
  ]

-- UPDATE

type Msg 
  = AddPawn Int
  

type alias Case =
  { state : CaseState
  , id : Int
  }

type CaseState 
  = Empty
  | PlayerOne
  | PlayerTwo

update : Msg -> Model -> Model
update msg model =
  case msg of
    AddPawn id ->
      model

-- VIEW   

view : Model -> Html Msg
view model =
  div [ ]
    [ div
        [ ]
        (List.map(\i -> button [ onClick (AddPawn i) ] [ text (String.fromInt i) ]) model.buttons)
    , div 
        [ class "gridContainer"
        , style "display" "flex"
        ]
        (List.map (\l -> div 
                            [ class "column" ]
                            (List.map(\n -> div
                                              [ if n.state == Empty then
                                                  class "E"
                                                else
                                                  class "N"
                                              ]
                                              [ if n.state == Empty then
                                                  text "E"
                                                else
                                                  text "N"
                                              ]) l)
                                          
          ) model.board)
    ]