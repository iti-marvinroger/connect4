import Browser
import List
import Html exposing (..)
import List.Extra exposing (..)
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
  [0, 1, 2, 3, 4, 5, 6] 
  [
    [ Empty, Empty, Empty, Empty, Empty, Empty ],
    [ PlayerOne, Empty, Empty, Empty, Empty, Empty ],
    [ PlayerTwo, Empty, Empty, Empty, Empty, Empty ],
    [ Empty, Empty, Empty, Empty, Empty, Empty ],
    [ Empty, Empty, Empty, Empty, Empty, Empty ],
    [ Empty, Empty, Empty, Empty, Empty, Empty ],
    [ Empty, Empty, Empty, Empty, Empty, Empty ]
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

caseString : CaseState -> String
caseString caseState = 
  case caseState of 
    Empty -> 
      "Empty"
    PlayerOne -> 
      "PlayerOne"
    PlayerTwo -> 
      "PlayerTwo"

update : Msg -> Model -> Model
update msg model =
  case msg of
    AddPawn id ->
      case List.Extra.getAt id model.board of
        Just h ->
          case List.head h of
            Just t ->
              case t of
                Empty ->
                  Debug.log(String.fromInt(id))
                  model
                PlayerOne ->
                  Debug.log("PlayerOne")
                  model
                PlayerTwo ->
                  Debug.log("PlayerTwo")
                  model
            Nothing ->
              model
        Nothing -> 
          model
        
-- VIEW   

view : Model -> Html Msg
view model =
  div [ ]
    [ div
        [ class "buttons" ]
        (List.map(\i -> button [ onClick (AddPawn i) ] [ text (String.fromInt i) ]) model.buttons)
    , div 
        [ class "gridContainer"
        , style "display" "flex"
        ]
        (List.map (\l -> div 
                            [ class "column" ]
                            (List.map(\n -> div
                                              [ 
                                                class (caseString n)
                                              ]
                                              [ 
                                                li [] []
                                              ]) l)
                                          
          ) model.board)
    ]