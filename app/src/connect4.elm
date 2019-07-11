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
    board : List Int
  }

init : Model
init =
  Model (List.range 1 42)

nums : List Int
nums = List.range 1 42


-- UPDATE

type Msg = Increment

update : Msg -> Model -> Model
update msg model =
  case msg of
    Increment ->
      model


-- VIEW   

view : Model -> Html Msg
view model =
  div [ ]
    [
      ul []
        (List.map (\l -> li [] [ text (String.fromInt l) ]) nums),
      button [ onClick Increment ] [ text "+" ]
    ]