let grade = [4.5; 7.0; 8.5; 3.0; 9.0]

let passedGrade = grade |> List.filter (fun g -> g >= 5.0)
let upGrade = grade |> List.map (fun g -> g + 0.5)
let averageGrade = grade |> List.sum |> fun sum -> sum / float (List.length grade)

printfn "Passed Grades: %A" passedGrade
printfn "Upgraded Grades: %A" upGrade
printfn "Average Grade: %.2f" averageGrade

let banana = "banana"
let charCounts =
    banana
    |> Seq.toList
    |> List.fold (fun acc ch ->
        let oldCount = Map.tryFind ch acc |> Option.defaultValue 0
        Map.add ch (oldCount + 1) acc
    ) Map.empty

printfn "Character counts in '%s': %A" banana charCounts