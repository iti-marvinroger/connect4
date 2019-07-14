# connect4

The project exposes an API at `http://127.0.0.1:8080/board`.

## Endpoints

### GET

Get the current state of the board.

```json
{
  "status": "Ongoing" | "Draw" | "Won",
  "lastPlayer": "None" | "Red" | "Yellow",
  "board": [ // array of columns (array of pawns)
    ["None" | "Red" | "Yellow", ...],
    ...
  ]
}
```

### POST

Play a move on the board with a `{ "column": 0, "dawn": "Red" | "Yellow" }`.
If the move is impossible or the request is malformed, we return a `{ "error": "Bad request" }`

The return value is the same as `GET`.

### DELETE

Reset the game. No payload.

The return value is the same as `GET`.

## Build and run front app

### Build

```
cd frontend/app
elm make src/connect4.elm --output=elm.js
``` 

### Run

Open the `index.html` file which includes CSS in your browser.