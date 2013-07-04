module IntradayBarDataRequest

open System
open System.Collections.Generic
open System.Linq
open System.Text

open BEmu //un-comment this line to use the Bloomberg API Emulator
//open Bloomberglp.Blpapi; //un-comment this line to use the actual Bloomberg API

let ProcessResponse (event : Event) (security : string) =
    //Note that the IntradayBarResponse does not include the name of the requested security anywhere
    printfn "%s" security
    
    event.GetMessages()
    |> Seq.iter (fun msg ->
        let elmBarTickDataArray = msg.["barData"].["barTickData"]
        for valueIndex in 0..elmBarTickDataArray.NumValues-1 do
            let elmBarTickData = elmBarTickDataArray.GetValueAsElement(valueIndex)

            let dtTick = elmBarTickData.GetElementAsDatetime("time").ToSystemDateTime()

            let open' = elmBarTickData.GetElementAsFloat64("open")
            let high = elmBarTickData.GetElementAsFloat64("high")
            let low = elmBarTickData.GetElementAsFloat64("low")
            let close = elmBarTickData.GetElementAsFloat64("close")

            let numEvents = elmBarTickData.GetElementAsInt32("numEvents")
            let volume = elmBarTickData.GetElementAsInt64("volume")
            let value = elmBarTickData.GetElementAsFloat64("value")

            printfn "%s" (dtTick.ToString("HH:mm:ss"))
            printfn "%s" (String.Format("\t open = {0:c2}", open'))
            printfn "%s" (String.Format("\t high = {0:c2}", high))
            printfn "%s" (String.Format("\t low = {0:c2}", low))
            printfn "%s" (String.Format("\t close = {0:c2}", close))
            printfn ""    
            printfn "%s" (String.Format("\t numEvents = {0:n0}", numEvents))
            printfn "%s" (String.Format("\t volume = {0:n0}", volume))
            printfn "%s" (String.Format("\t value = {0:n0}", value))

            printfn ""
    )

let IntradayBarDataRequestExample() =

    let uri = "//blp/refdata"
    let operationName = "IntradayBarRequest"
    let sessionOptions = new SessionOptions(ServerHost = "127.0.0.1", ServerPort = 8194)
    let session = new Session(sessionOptions)

    if session.Start() && session.OpenService(uri) then
        let service = session.GetService("//blp/refdata")
        let request = service.CreateRequest("IntradayBarRequest")

        let security = "SPY US EQUITY"
        request.Set("security", security) //required        

        request.Set("eventType", "TRADE") //optional: TRADE(default), BID, ASK, BID_BEST, ASK_BEST, BEST_BID, BEST_ASK, BID_YIELD, ASK_YIELD, MID_PRICE, AT_TRADE, SETTLE
        //note that BID_YIELD, ASK_YIELD, MID_PRICE, AT_TRADE, and SETTLE don't appear in the API documentation, but you will see them if you call "service.ToString() using the actual Bloomberg API"
        request.Set("eventTypes", "BID") //A request can have multiple eventTypes

        //data goes back no farther than 140 days (7.2.4)
        let dtStart = DateTime.Today.AddDays(-1.) //yesterday
        request.Set("startDateTime", new Datetime(dtStart.AddHours(9.5).ToUniversalTime())) //Required Datetime, UTC time
        request.Set("endDateTime", new Datetime(dtStart.AddHours(16.).ToUniversalTime())) //Required Datetime, UTC time

        //(Required) Sets the length of each time bar in the response. Entered as a whole number, between 1 and 1440 in minutes.
        //  One minute is the lowest possible granularity. (despite A.2.8, the interval setting cannot be omitted)
        request.Set("interval", 60)

        //When set to true, a bar contains the previous bar values if there was no tick during this time interval.
        request.Set("gapFillInitialBar", false) //Optional bool. Valid values are true and false (default = false)

        //Option on whether to return EIDs for the security.
        request.Set("returnEids", false) //Optional bool. Valid values are true and false (default = false)

        //Setting this to true will populate fieldData with an extra element containing a name and value for the relative date. For example RELATIVE_DATE = 2002 Q2
        request.Set("returnRelativeDate", false) //Optional bool. Valid values are true and false (default = false)

        //Adjust historical pricing to reflect: Regular Cash, Interim, 1st Interim, 2nd Interim, 3rd Interim, 4th Interim, 5th Interim, Income,
        //  Estimated, Partnership Distribution, Final, Interest on Capital, Distribution, Prorated.
        request.Set("adjustmentNormal", false) //Optional bool. Valid values are true and false (default = false)

        //Adjust historical pricing to reflect: Special Cash, Liquidation, Capital Gains, Long-Term Capital Gains, Short-Term Capital Gains, Memorial,
        //  Return of Capital, Rights Redemption, Miscellaneous, Return Premium, Preferred Rights Redemption, Proceeds/Rights, Proceeds/Shares, Proceeds/Warrants.
        request.Set("adjustmentAbnormal", false) //Optional bool. Valid values are true and false (default = false)

        //Adjust historical pricing and/or volume to reflect: Spin-Offs, Stock Splits/Consolidations, Stock Dividend/Bonus, Rights Offerings/Entitlement.
        request.Set("adjustmentSplit", false) //Optional bool. Valid values are true and false (default = false)

        //Setting to true will follow the DPDF<GO> BLOOMBERG PROFESSIONAL service function. True is the default setting for this option..
        request.Set("adjustmentFollowDPDF", false) //Optional bool. Valid values are true and false (default = false)

        session.SendRequest(request, CorrelationID(-999L)) |> ignore

        let rec ReadResults() =
            let event = session.NextEvent()
            match event.Type with
            | Event.EventType.RESPONSE ->
                ProcessResponse event security
                ()
            | Event.EventType.PARTIAL_RESPONSE ->
                ProcessResponse event security
                ReadResults()
            | _ ->
                ReadResults() // C# version doesn't handle this case - F# pattern matching FTW.

        ReadResults()

[<EntryPoint>]
let main args =
    IntradayBarDataRequestExample()
    Console.WriteLine("Press a key")
    Console.ReadKey(true) |> ignore
    0
    