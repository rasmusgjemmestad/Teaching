(**
[![Binder](img/badge-binder.svg)](https://mybinder.org/v2/gh/nhirschey/teaching/gh-pages?filepath=portfolio-statistics.ipynb)&emsp;
[![Script](img/badge-script.svg)](/Teaching//portfolio-statistics.fsx)&emsp;
[![Notebook](img/badge-notebook.svg)](/Teaching//portfolio-statistics.ipynb)

This page covers important fundamentals for building portfolios.

* [Portfolio Weights](#Portfolio-Weights)

* [Mean and Variance of Portfolio Returns](#Mean-and-Variance-of-Portfolio-Returns)

* [Leverage](#Leverage)

## Portfolio Weights

An investment portfolio consists of positions in assets.
It is common to refer to a position's size as its share of
the portfolio's total value.
This is known as the asset's portfolio weight.

The portfolio weight of asset $i$ in portfolio $p$ is

$$w_i=(\text{positionValue}_i)/(\text{portfolioValue}_p)$$.

### Long positions

When an investor buys a long position,
they pay for the position now and hope to sell it later at a higher price.

Let's look at cash flows for long positions.

* At time 0: investor has $100 cash and no shares.

* At time 1: investor takes their $100 of cash and buys 4 shares 
of stock at a price of $25 per share. They have no cash and are long 4 shares.

* At time 2: the stock has risen to $27, the investor sells their 4 shares.
They have $108 of cash and no shares.

We can define some functions to update an account given these trades.

*)
// A record type that defines an account
type AccountBalances =
    { Time: int
      Cash: float 
      Shares: float }

// A record type that defines a trade
type Trade = 
    { Shares: float 
      Price : float }

let accountAt0 = { Time = 0; Cash = 100.0; Shares = 0.0 }
let tradeAt1 = { Shares = 4.0; Price = 25.0 }

// `updateAccount` is a function that updates an account after a trade is made.
// 
// (trade: Trade) restricts the `trade` parameter to data of type `Trade`.
//
// (inAccount: AccountBalances) restricts the `inAccount` parameter 
// to data of type `AccountBalances`
//
let updateAccount (trade: Trade) (inAccount: AccountBalances) =
    let tradeValue = trade.Price * trade.Shares
    let newCash = inAccount.Cash - tradeValue
    let newShares = inAccount.Shares + trade.Shares
    let newTime = inAccount.Time + 1
    { Time = newTime 
      Cash = newCash 
      Shares = newShares }
(**
You can make names with spaces using `` before and after.

*)
let ``expected account at t1`` = { Time = 1; Cash = 0.0; Shares = 4.0}
let ``actual account at t1`` = updateAccount tradeAt1 accountAt0 

``actual account at t1``(* output: 
{ Time = 1
  Cash = 0.0
  Shares = 4.0 }*)
if ``actual account at t1`` <> ``expected account at t1`` then
    failwith "You are not updating account correctly after a trade"
(**
Now we can calculate how the account value changes over time.

*)
let accountAt1 = updateAccount tradeAt1 accountAt0

accountAt1(* output: 
{ Time = 1
  Cash = 0.0
  Shares = 4.0 }*)
let tradeAt2 = { Shares = -4.0; Price = 27.0 }
let accountAt2 = updateAccount tradeAt2 accountAt1

accountAt2(* output: 
{ Time = 2
  Cash = 108.0
  Shares = 0.0 }*)
(**
We could have also written this code using the pipe operator.

*)
let accountAt1' = accountAt0 |> updateAccount tradeAt1 // same as "updateAccount tradeAt1 accountAt0"
let accountAt2' = accountAt1 |> updateAccount tradeAt2 // same as "updateAccount tradeAt2 accountAt1"

accountAt1'(* output: 
{ Time = 1
  Cash = 0.0
  Shares = 4.0 }*)
accountAt2'(* output: 
{ Time = 2
  Cash = 108.0
  Shares = 0.0 }*)
(**
The pipe operator does not look very useful above because
we are only doing one operation.
It is more useful when you're doing a series of multiple operations.
This example recomputes the account value at time 2 by
chaining together updates for the trades at times 1 and 2.

*)
let accountAt2'' =
    accountAt0
    |> updateAccount tradeAt1
    |> updateAccount tradeAt2

accountAt2''(* output: 
{ Time = 2
  Cash = 108.0
  Shares = 0.0 }*)
(**
This code is closer to how you would describe it in English:
"Start with the account at time 0,
update it for the trade at time 1,
then update it for the trade at time 2."

> Practice: complete the code for the `accountValue` function below.
It should calculate total account value of
the stock and cash positiions.
If it is correct then the account value test below should evaluate to `true`
> 

*)
let accountValue (stockPrice: float) (account: AccountBalances) =
    failwith "unimplemented"
// simple account value test
(accountValue 27.0 accountAt2) = 108.0
(**
### Portfolio weights of long positions

Now that we understand long positions we can calculate portfolio weights for them.
Let's calculate weights for an example **Portfolio A** consisting of

* $100 invested in AAPL

* $300 invested in GOOG

* $500 invested in TSLA

These are all long positions, meaning that they have positive costs.

*)
let aaplPositionValue = 100.0
let googPositionValue = 300.0
let tslaPositionValue = 500.0

// This implies:

let portfolioValueA = aaplPositionValue + googPositionValue + tslaPositionValue

portfolioValueA(* output: 
900.0*)
(**
The portfolio weights are then

*)
let aaplWeight = aaplPositionValue / portfolioValueA

aaplWeight(* output: 
0.1111111111*)
let googWeight = googPositionValue / portfolioValueA

googWeight(* output: 
0.3333333333*)
let tslaWeight = tslaPositionValue / portfolioValueA

tslaWeight(* output: 
0.5555555556*)
(**
These weights for AAPL, GOOG, and TSLA are all positive.
Long positions always have positive weights.

Another thing to notice is that the portfolio weights add up to one (or 100%):

*)
aaplWeight + googWeight + tslaWeight(* output: 
val it : float = 1.0*)
(**
This portfolio is a net long portfolio,
meaning that it costs you money to purchase it.
Net long portfolios such as this one must
have portfolio weights that add up to one.
Due to margin requirements, real-world
portfolios are generally net long--you must
put up capital to acquire the portfolio.

The other type of portfolio is a zero-cost portfolio.
As the name implies, zero-cost portfolios do not require any investment to purchase.
There is no cost because long positions are funded by offsetting short positions.
To see how this works we need to examine how short positions work.

### Short positions

When an investor buys a long position,
they pay for the position now and hope to sell it later at a higher price.
A short sale reverses this.
The investor sells the position now and hopes to buy it back later at a lower price.

We now go through an example to see how the cash flows work.

* At time 0: investor has $100 cash and no shares.

* At time 1: investor borrows 4 shares of stock and sells them
for a price of $25 per share. They have $200 cash and are short 4 shares.

* At time 2: the stock has risen to $27, the investor buys back the
4 shares that they sold short and returns them to the person that
they borrowed them from. They have $92 of cash and 0 shares.

The investor's cash and stock balances at the end of each period will look something like

*)
let shortAt1 = { Shares = -4.0; Price = 25.0 }
let shortCoverAt2 = { Shares = 4.0; Price = 27.0 }

// positions at t1
accountAt0 
|> updateAccount shortAt1(* output: 
{ Time = 1
  Cash = 200.0
  Shares = -4.0 }*)
// positions at t2
accountAt0 
|> updateAccount shortAt1 
|> updateAccount shortCoverAt2(* output: 
{ Time = 2
  Cash = 92.0
  Shares = 0.0 }*)
(**
### Portfolio weights for short positions

Let's create a new portfolio, **Portfolio B**, that includes short sales and calculate weights. Assume that you start with **Portfolio A** and short $150 of AMZN stock. This generates $150 of cash that you have to put somewhere. For individual investors, often your broker puts it in bonds and gives you none of the interest. Institutional investors can get some of the interest or even reinvest the proceeds in something else. We will assume that we are an institution and can reinvest all of the short proceeds. We will take the $150 and add $50 to each of our AAPL, GOOG, and TLSA positions.

Short positions have negative portfolio weights.

*)
let amznPositionValueB = -150.0
let aaplPositionValueB = aaplPositionValue + 50.0
let googPositionValueB = googPositionValue + 50.0
let tslaPositionValueB = tslaPositionValue + 50.0

let portfolioValueB = 
    amznPositionValueB +
    aaplPositionValueB +
    googPositionValueB +
    tslaPositionValueB

portfolioValueB(* output: 
900.0*)
(**
Compare to **Portfolio A**

*)
portfolioValueA = portfolioValueB (* output: 
val it : bool = true*)
(**
The weights in **Portfolio B**:

*)
let amznWeightB = amznPositionValueB / portfolioValueB

amznWeightB(* output: 
-0.1666666667*)
let aaplWeightB = aaplPositionValueB / portfolioValueB

aaplWeightB(* output: 
0.1666666667*)
let googWeightB = googPositionValueB / portfolioValueB

googWeightB(* output: 
0.3888888889*)
let tslaWeightB = tslaPositionValueB / portfolioValueB

tslaWeightB(* output: 
0.6111111111*)
(**
The weights of **Portfolio B** also add up to one.

*)
amznWeightB + aaplWeightB + googWeightB + tslaWeightB(* output: 
val it : float = 1.0*)
(**
### Zero-cost portfolios

Another type of portfolio that you will see is zero-cost portfolios.
They are called self funding because the short sale proceeds
fund the long investments. The portfolio weights add up to 0.
You can scale weights relative to what they would be per $ long or short.

An example:

*)
// Portfolio C
let koPositionValue = -50.0
let hogPositionValue = 40.0
let yumPositionValue = 10.0

let dollarsLong = 50.0
let koWeight = koPositionValue / dollarsLong
let hogWeight = hogPositionValue / dollarsLong
let yumWeight = yumPositionValue / dollarsLong

printfn $"koWeight = {koWeight}"
printfn $"hogWeight= {hogWeight}"
printfn $"yumWeight= {yumWeight}"(* output: 
koWeight = -1
hogWeight= 0.8
yumWeight= 0.2*)
koWeight + hogWeight + yumWeight(* output: 
5.551115123e-17*)
(**
### Calculating weights using a list of positions

The calculations that we did thus far required code for each position.
We did the same thing to each position, so there was some repetition.
We can reduce the repetition by putting the positions into a list
and then operating on the elements of the list via iteration.

*)
// defining a position record type
type Position = { Id: string; PositionValue: float }

// assigning a list of positions to a value named portfolio
let portfolio =
    [ { Id = "AMZN"; PositionValue = amznPositionValueB }
      { Id = "AAPL"; PositionValue = aaplPositionValueB }
      { Id = "GOOG"; PositionValue = googPositionValueB }
      { Id = "TSLA"; PositionValue = tslaPositionValueB } ]

// This is called a list comprehension
let positionValues = [ for p in portfolio -> p.PositionValue ](* output: 
[-150.0; 150.0; 350.0; 550.0]*)
(**
The list module has many different functions for operating on lists.
If you type `List.` you should see many different functions pop up.
These functions are very useful. We will explore them in more detail later.

For now, let's see what `List.map` does.

*)
portfolio |> List.map (fun p -> p.PositionValue)(* output: 
val it : float list = [-150.0; 150.0; 350.0; 550.0]*)
(**
This is the same result as the `positionValues` value that we calculated
using the list comprehension.
`List.map` "maps" each element of the list to an output using a function.
In this case, our function `(fun p -> p.PositionValue)` was an anonymous function.

Another useful function from the list module is `List.sum`.
We can use it to calculate the total value of the portfolio by
summing position values.

*)
let portfolioValue = positionValues |> List.sum(* output: 
900.0*)
(**
And with this we can calculate portfolio weights.

*)
let portfolioWeights =
    [ for p in portfolio -> 
        let weight = p.PositionValue / portfolioValue 
        p.Id, weight ]
portfolioWeights(* output: 
[("AMZN", -0.1666666667); ("AAPL", 0.1666666667); ("GOOG", 0.3888888889);
 ("TSLA", 0.6111111111)]*)
(**
## Mean and Variance of Portfolio Returns

### A portfolio's return.

A portfolio's return is the weighted average return of the portfolio's positions.

\begin{equation}
 r_p = \Sigma^N_{i=1} w_i r_i,
\end{equation}

where $r$ is return, $i$ indexes stocks, and $w$ is portfolio weights.

*)
type PositionsWithReturn =
    { Id: string 
      Weight: float 
      Return: float }

let exPortfolio =
    [ { Id = "A"; Weight = 0.25; Return = 0.1 }
      { Id = "B"; Weight = 0.75; Return = 0.2 } ]

let weightsXreturn = [ for pos in exPortfolio -> pos.Weight * pos.Return ]
weightsXreturn(* output: 
[0.025; 0.15]*)
let exPortfolioReturn = weightsXreturn |> List.sum 
exPortfolioReturn(* output: 
0.175*)
(**
For next time: Portfolio Variance and Leverage

## Leverage

*)

