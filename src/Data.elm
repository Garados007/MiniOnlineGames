module Data exposing (..)

type alias GameRoom =
    { cardCount: Int
    , player: List String
    }

type alias GameCard =
    { issuer: String
    , job: String
    }

type alias Game =
    { room: GameRoom
    , currentCard: Maybe GameCard
    , currentDice: Maybe Int
    }
