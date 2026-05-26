
module Calculator =
    let add x y = x + y
    let sub x y = x - y
    let mul x y = x * y
    let div x y = x / y

printfn "Addition: %d" (Calculator.add 5 3)
printfn "Subtraction: %d" (Calculator.sub 5 3)
printfn "Multiplication: %d" (Calculator.mul 5 3)
printfn "Division: %d" (Calculator.div 5 3)


module Student =
    // type Name = string
    // type Score = float

    type Define = {
        Name: string
        Score: float
    }

    let isPassed score = score >= 5.0

let studentList: Student.Define list = [
    { Name = "Alice"; Score = 7.5 }
    { Name = "Bob"; Score = 4.0 }
    { Name = "Charlie"; Score = 6.0 }
]

printfn "Students who passed: %A" (studentList
|> List.filter (fun s -> Student.isPassed s.Score)
|> List.map (fun s -> s.Name))