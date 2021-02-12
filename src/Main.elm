port module Main exposing (..)

import Data
import Network exposing (Response (..))
import Browser
import Browser.Navigation
import Html exposing (Html)
import WebSocket
import Json.Decode as JD
import Json.Encode as JE
import Url exposing (Url)
import Url.Parser exposing ((</>))
import Maybe.Extra
import Task
import Random
import Time

port receiveSocketMsg : (JD.Value -> msg) -> Sub msg
port sendSocketCommand : JE.Value -> Cmd msg

main : Program () Model Msg
main = Browser.application
    { init = \() -> init
    , view = \model ->
        { title = "Auftragskarten"
        , body = view model
        }
    , update = update
    , subscriptions = subscriptions
    , onUrlRequest = always Noop
    , onUrlChange = always Noop
    }

type Msg
    = Network Network.Response
    | Noop
    | WsMsg (Result JD.Error WebSocket.WebSocketMsg)
    | DoRoll
    | SetDice Int

type alias Model =
    { game: Maybe Data.Game
    , key: Browser.Navigation.Key
    , token: String
    , rolling: Bool
    , nameInput: String
    , diceNum: Int
    }

init : Url -> Browser.Navigation.Key -> (Model, Cmd Msg)
init url key =
    let
        token : String
        token = getId url |> Maybe.withDefault ""
    in Tuple.pair
        { game = Nothing
        , key = key
        , token = token
        , rolling = False
        , nameInput = ""
        , diceNum = 1
        }
        <| Debug.log "init cmd"
        <| Cmd.batch
            [ WebSocket.send sendSocketCommand
                <| Debug.log "ws connect"
                <| WebSocket.Connect
                    { name = "wss:" ++ token
                    , address = 
                        "wss://" ++ url.host ++
                        (Maybe.map
                            ((++) ":" << String.fromInt)
                            url.port_
                            |> Maybe.withDefault ""
                        ) ++
                        "/ws/task-game/" ++ token
                    , protocol = ""
                    }
            , Task.perform identity
                <| Task.succeed DoRoll
            ]

view : Model -> List (Html msg)
view model =
    List.singleton <| Html.text <| Debug.toString model

update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
    case msg of
        Network (RespSendCard issuer job) ->
            Tuple.pair
                { model
                | game = Maybe.map
                    (\game ->
                        { game
                        | currentCard = Just 
                            <| Data.GameCard
                                issuer
                                job
                        }
                    )
                    model.game
                }
                Cmd.none
        Network (RespSendDiceResult dice) ->
            Tuple.pair
                { model
                | game = Maybe.map
                    (\game ->
                        { game
                        | currentDice = Just dice
                        }
                    )
                    model.game
                , rolling = False
                }
                Cmd.none
        Network (RespSendGameInfo room) ->
            Tuple.pair
                { model
                | game = model.game
                    |> Maybe.map
                        (\game ->
                            { game
                            | room = room
                            }
                        )
                    |> Maybe.Extra.orElseLazy
                        (\() -> Just
                            { room = room
                            , currentCard = Nothing
                            , currentDice = Nothing
                            }
                        )
                }
                Cmd.none
        Network (RespStartRoolDice) ->
            Tuple.pair 
                { model | rolling = True }
                Cmd.none
        Noop -> (model, Cmd.none)
        WsMsg (Ok (WebSocket.Data d)) ->
            Tuple.pair model
                <| Task.perform identity
                <| (\result ->
                        case result of
                            Ok v -> Task.succeed <| Network v
                            Err _ -> Task.succeed Noop
                    )
                <| JD.decodeString
                    Network.decodeResponse
                    d.data
        WsMsg (Ok _) -> (model, Cmd.none)
        WsMsg (Err _) -> (model, Cmd.none)
        DoRoll ->
            Tuple.pair model
                <| Random.generate SetDice
                <| Random.int 1 6
        SetDice num ->
            Tuple.pair
                { model | diceNum = num }
                Cmd.none

subscriptions : Model -> Sub Msg
subscriptions game =
    if game.rolling
    then Time.every 100 <| always DoRoll
    else Sub.none

getId : Url -> Maybe String
getId =
    Url.Parser.parse
        <| Url.Parser.s "game"
        </> Url.Parser.string
