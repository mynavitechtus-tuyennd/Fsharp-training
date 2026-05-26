// Homework: Option and Mutable State
// In this homework, you will practice using the Option type and mutable state in F#.
// 1. Create a function `tryLogin` that takes a username and returns an Option<string> representing a token if the login is successful, or None if it fails. For simplicity, assume that the only valid username is "admin" and the token is "token-123".
let tryLogin username =
    if username = "admin" then Some "token-123"
    else None

let printResult username = 
    match tryLogin username with
    | Some token -> printfn "Logged in with token: %s" token
    | None -> printfn "Login failed"

printResult "admin"
printResult "user"


// 2. Create a mutable variable `counter` that keeps track of the number of login attempts. Increment this counter each time the `tryLogin` function is called, regardless of whether the login was successful or not.
let mutable count = 0
count <- count + 1
count <- count + 1
count <- count + 1

// printfn "Final count of login attempts: %d" count