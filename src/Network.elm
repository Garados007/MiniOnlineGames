module Network exposing (..)

import Data exposing (..)
import Json.Encode as JE
import Json.Decode as JD exposing (Decoder, Value)
import Json.Decode.Pipeline exposing (required)

type Request
    = ReqSendText String
    | ReqSetName String
    | ReqStartRollDice

type Response
    = RespSendCard String String
    | RespSendDiceResult Int
    | RespSendGameInfo GameRoom
    | RespStartRoolDice

encodeRequest : Request -> Value
encodeRequest request =
    case request of
        ReqSendText message ->
            JE.object
                [ Tuple.pair "$type" <| JE.string "SendText"
                , Tuple.pair "message" <| JE.string message
                ]
        ReqSetName name ->
            JE.object
                [ Tuple.pair "$type" <| JE.string "SetName"
                , Tuple.pair "name" <| JE.string name
                ]
        ReqStartRollDice ->
            JE.object
                [ Tuple.pair "$type" <| JE.string "StartRollDice" 
                ]

decodeResponse : Decoder Response
decodeResponse =
    JD.andThen
        (\type_ ->
            case type_ of
                "SendCard" ->
                    JD.succeed RespSendCard
                        |> required "initiator" JD.string
                        |> required "message" JD.string
                "SendDiceResult" ->
                    JD.succeed RespSendDiceResult
                        |> required "result" JD.int
                "SendGameInfo" ->
                    JD.succeed GameRoom
                        |> required "card-count" JD.int
                        |> required "player" (JD.list JD.string)
                        |> JD.map RespSendGameInfo
                "StartRollDice" ->
                    JD.succeed RespStartRoolDice
                _ -> JD.fail "unknown event"
        )
    <| JD.field "$type" JD.string
