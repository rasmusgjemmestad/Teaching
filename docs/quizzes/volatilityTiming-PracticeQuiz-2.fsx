
(**
We're going to use the following in the questions
*)

#r "nuget: FSharp.Stats"

open System
open FSharp.Stats

fsi.AddPrinter<DateTime>(fun dt -> dt.ToString("s"))

type ReturnOb = { Symbol : string; Date : DateTime; Return : float }
type ValueOb = { Symbol : string; Date : DateTime; Value : float }

let seed = 1
Random.SetSampleGenerator(Random.RandBasic(seed))   
let normal = Distributions.Continuous.normal 0.0 0.1

let returns =
    [| 
        for symbol in ["AAPL"; "TSLA"] do
        for month in [1..2] do
        for day in [1..3] do
            { Symbol = symbol 
              Date = DateTime(2021, month, day)
              Return = normal.Sample()}
    |]

(*** define:collect ***)
(*
    Take this array of arrays, add 1.0 to each element of the "inner" arrays,
    and then concatenate all the inner arrays together.
    [| [| 1.0; 2.0|]
       [| 3.0; 4.0|] |]
*)

(*** define:collect-ans ***)
[| [| 1.0; 2.0|]
   [| 3.0; 4.0|] |]
|> Array.collect(fun xs -> xs |> Array.map(fun x -> x + 1.0))
// or
[| [| 1.0; 2.0|]
   [| 3.0; 4.0|] |]
|> Array.map(fun xs -> xs |> Array.map(fun x -> x + 1.0))
|> Array.concat

(*** define:partialApplication ***)

(*
    Take the following two-parameter function:
    ```
        let add x y = x + y
    ```
    Use the above function and [partial application](https://fsharpforfunandprofit.com/posts/partial-application/)
    to define a new function called 
    `add2` that adds 2 
    to it's input.
*)

(*** define:partialApplication-ans ***)
let add x y = x + y
let add2 = add 2

(*** define:printfnStructuredObject ***)
(*
    Given `returns : ReturnOb []`, use printfn to print the whole
    array to standard output using the [structured plaintext formatter](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/plaintext-formatting). 
*)

(*** define:printfnStructuredObject-ans ***)
returns |> (printfn "%A")

(*** define:printfnStructuredRecord ***)
(*
    Given `returns : ReturnOb []`, use printfn to print each record
    to standard output using the [structured plaintext formatter](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/plaintext-formatting). 
*)

(*** define:printfnStructuredRecord-ans ***)
returns |> Array.iter (printfn "%A")

(*** define:printfnStringInterpolation ***)
(*
    Given the tuple ("hi", false, 20.321, 4),
    use printfn and the tuple to print the following string
    to standard output:

    "hi teacher, my False knowledge implies that 4%=0020.1"

    [String formatting](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/plaintext-formatting#format-specifiers-for-printf) documentation will be useful. 
*)

(*** define:printfnStringInterpolation-ans ***)
let (xString, xBool, xFloat, xInt) = ("hi", false, 20.321, 4)
// Using string interpolation
printfn $"{xString} teacher, my {xBool} knowledge imples that {xInt}%%=%06.1f{xFloat}"
// Using old-style printfn
printfn "%s teacher, my %b knowledge imples that %i%%=%06.1f" xString xBool xInt xFloat

(*** define: arithmeticReturn ***)
(*
    Given `returns : ReturnOb []`, calculate the arithmetic average return 
    for every symbol each month.
    Give the result as a `ReturnOb []` where the date is the last date for the symbol
    each month. 
*)

(*** define: arithmeticReturn-ans ***)
returns
|> Array.groupBy(fun x -> x.Symbol, x.Date.Year, x.Date.Month)
|> Array.map(fun ((symbol, _year, _month), xs) ->
    { Symbol = symbol 
      Date = xs |> Array.map(fun x -> x.Date) |> Array.max 
      Return = xs|> Array.averageBy(fun x -> x.Return) })

(*** define: monthlyReturn ***)
(*
    Given `returns : ReturnOb []`, calculate the monthly return 
    for every symbol each month.
    Give the result as a `ReturnOb []` where the date is the last date for the symbol
    each month. 
*)

(*** define: monthlyReturn-ans ***)
returns
|> Array.groupBy(fun x -> x.Symbol, x.Date.Year, x.Date.Month)
|> Array.map(fun ((symbol, _year, _month), xs) ->
    let monthReturnPlus1 = (1.0, xs) ||> Array.fold(fun acc x -> acc * (1.0 + x.Return))
    { Symbol = symbol 
      Date = xs |> Array.map(fun x -> x.Date) |> Array.max 
      Return =  monthReturnPlus1 - 1.0 })

(*** define: monthlyStdDev ***)
(*
    Given `returns : ReturnOb []`, calculate the standard deviation of daily returns
    for every symbol each month.
    Give the result as a `ValueOb []` where the date in each `ValueOb` is the last date for the symbol
    each month. 
*)

(*** define: monthlyStdDev-ans ***)
returns
|> Array.groupBy(fun x -> x.Symbol, x.Date.Year, x.Date.Month)
|> Array.map(fun ((symbol, _year, _month), xs) ->
    let sd = xs |> stDevBy(fun x -> x.Return)
    { Symbol = symbol 
      Date = xs |> Array.map(fun x -> x.Date) |> Array.max 
      Value =  sd })

(*** define: rollingStdDev ***)
(*
    Given `returns : ReturnOb []`, calculate the standard deviation of daily returns
    for every symbol using rolling 3 day windows.
    Give the result as a `ValueOb []` where the date in each `ValueOb` is the last date for the symbol
    in the window. 
*)

(*** define: rollingStdDev-ans ***)
returns
|> Array.groupBy(fun x -> x.Symbol)
|> Array.collect(fun (_symbol, xs) ->
    xs
    |> Array.sortBy(fun x -> x.Date)
    |> Array.windowed 3
    |> Array.map(fun ys -> 
        let last = ys |> Array.last 
        { Symbol = last.Symbol
          Date = last.Date
          Value = ys |> stDevBy(fun x -> x.Return)}))

// breaking this answer down,
// If you're unsure, it's helpful to work through things step by step.
// then build up from there.

let groups = 
    returns
    |> Array.groupBy(fun x -> x.Symbol)

let firstGroup = groups |> Array.item 0 // or groups |> Array.head
let firstSymbol, firstObs = firstGroup // like let a,b = (1,2)
let windowedFirstObs = 
    firstObs
    |> Array.sortBy(fun x -> x.Date)
    |> Array.windowed 3
let firstWindow = windowedFirstObs.[0]
let lastDayOfFirstWindow = firstWindow |> Array.last
let firstWindowReturnStdDev = firstWindow |> stDevBy(fun x -> x.Return)
let firstWindowResult =
    { Symbol = lastDayOfFirstWindow.Symbol 
      Date = lastDayOfFirstWindow.Date
      Value = firstWindowReturnStdDev }

// Now take the inner-most code operating on a single window
// and make a function by copying and pasting inside a function.
// often using more general variable names
let resultForWindow window =
    let lastDay = window |> Array.last
    let stddev = window |> stDevBy(fun x -> x.Return)
    { Symbol = lastDay.Symbol 
      Date = lastDay.Date
      Value = stddev }
// test it on your window
let firstWindowFunctionResult = resultForWindow firstWindow
// check
firstWindowResult = firstWindowFunctionResult // evaluates to true
// now a function to create the windows
let createWindows (days: ReturnOb array) =
    days
    |> Array.sortBy(fun day -> day.Date)
    |> Array.windowed 3
// check
(createWindows firstObs) = windowedFirstObs // evaluates to true
// so now we can do
firstObs
|> createWindows
|> Array.map resultForWindow
// Cool, now first obs was the obs from the first group.
// we could do function to operate on a group.
// our group is a tuple of `(string,ReturnObs array)`.
// We're not going to use the string variable, so we'll preface it
// with _ to let the compiler know we're leaving it out o purpose.
// the _ is not necessary but it's good practice
let resultsForGroup (_symbol, xs) =
    xs
    |> createWindows
    |> Array.map resultForWindow
// test it on the first group
resultsForGroup firstGroup
// now make the group and apply my 
// group function to each group
let resultsForEachGroup =
    returns
    |> Array.groupBy(fun x -> x.Symbol)
    |> Array.map resultsForGroup
// Okay, but this is an array of ValueOb arrays (that's what ValuOb [][] means)
// What happened is that I had an array of groups, and then I transformed each group.
// so it's still one result per group. For instance
resultsForEachGroup.[0]
// is the first group of results
resultsForEachGroup.[1]
// is the second group. I don't want an array of arrays.
// I just want one array of value obs. So concat them.
let resultsForEachGroupConcatenated =
    resultsForEachGroup |> Array.concat
// what's the first thing in the array?
resultsForEachGroupConcatenated.[0]    
// Collect does the map and concat in one step.
let resultsForEachGroupCollected =
    returns
    |> Array.groupBy(fun x -> x.Symbol)
    |> Array.collect resultsForGroup
// check, this should evaluate to true
resultsForEachGroupConcatenated.[0] = resultsForEachGroupCollected.[0]
// why did I write the answer using an anonymous function instead of functions like this?
// I use reusable functions for something I'm going to use multiple times.
// If it's something I'll do once, and it's not too many lines, then I use
// the anonymous lambda function. As you get more experience, you can code using
// the type signatures to tell you what everything is. And I don't actually
// have to running it step by step.
// however, starting out especially, I think you'll find it helpful
// to kinda break things down like I did here.
//
// Another way you can do it, similar to the first answer using
// an anonymous lambda function, but now we'll do it with fewer
// nested arrays by concatenating/collecting the windows
// into the parent array before doing the standard deviations.
let m2Groups =
    returns
    |> Array.groupBy(fun x -> x.Symbol)

let m2GroupsOfWindows =
    m2Groups
    |> Array.map(fun (symbol, xs) -> 
        xs
        |> Array.sortBy(fun x -> x.Date)
        |> Array.windowed 3
    )
// first group of windows
m2GroupsOfWindows.[0]    
// second group of windows
m2GroupsOfWindows.[1]    
// Now concatenate the windows.
let m2GroupsOfWindowsConcatenated = m2GroupsOfWindows |> Array.concat
 // 
// same as if I'd used collect instead of map and then concat
let m2GroupsOfWindowsCollected =
    m2Groups
    |> Array.collect(fun (symbol, xs) -> 
        xs
        |> Array.sortBy(fun x -> x.Date)
        |> Array.windowed 3
    )
// compare them
let m2FirstConcatenated = m2GroupsOfWindowsConcatenated.[0]    
let m2FirstCollected = m2GroupsOfWindowsCollected.[0]
m2FirstCollected = m2FirstConcatenated // true. 
// If they're not true, make sure they're sorted the same before you take the first obs.

// Now, standard deviations of the windows' returns
let m2Result =
    m2GroupsOfWindowsCollected
    |> Array.map(fun window -> 
        let lastDay = window |> Array.last 
        { Symbol = lastDay.Symbol
          Date = lastDay.Date
          Value = window |> stDevBy(fun x -> x.Return )})

(**
## Questions 
*)

(*** include: collect ***)

(*** include: partialApplication ***)

(*** include: printfnStructuredObject ***)

(*** include: printfnStructuredRecord ***)

(*** include:printfnStringInterpolation ***)

(*** include: arithmeticReturn ***)

(*** include: monthlyReturn ***)

(*** include: monthlyStdDev ***)

(*** include: rollingStdDev ***)

(**
## Answers 
*)

(*** include:collect ***)
(*** include: collect-ans ***)

(*** include: partialApplication ***)
(*** include: partialApplication-ans ***)

(*** include: printfnStructuredObject ***)
(*** include: printfnStructuredObject-ans ***)

(*** include: printfnStructuredRecord ***)
(*** include: printfnStructuredRecord-ans ***)

(*** include:printfnStringInterpolation ***)
(*** include:printfnStringInterpolation-ans ***)

(*** include: arithmeticReturn ***)
(*** include: arithmeticReturn-ans ***)

(*** include: monthlyReturn ***)
(*** include: monthlyReturn-ans ***)

(*** include: monthlyStdDev ***)
(*** include: monthlyStdDev-ans ***)

(*** include: rollingStdDev ***)
(*** include: rollingStdDev-ans ***)