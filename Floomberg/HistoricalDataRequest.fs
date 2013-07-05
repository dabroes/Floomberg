module HistoricalDataRequest

// Based on HistoricalDataRequest.cs from BEmu's Examples project

open System
open BEmu
//open Bloomberglp.Blpapi //un-comment this line to use the actual Bloomberg API

let HandleResponseEvent (eventObj : Event) =
    printfn "EventType = %s" (eventObj.Type.ToString())
    eventObj.GetMessages()
    |> Seq.iter (fun m -> printfn ""
                          printfn "correlationID = %A" m.CorrelationID
                          printfn "messageType = %A" m.MessageType

                          let elmSecurityData = m.["securityData"]
                          let elmSecurity = elmSecurityData.["security"]
                          let security = elmSecurity.GetValueAsString()

                          printfn "%s" security

                          let elmFieldData = elmSecurityData.["fieldData"]

                          for valueIndex in 0..elmFieldData.NumValues-1 do
                              let elmValues = elmFieldData.GetValueAsElement(valueIndex)
                              let date = elmValues.GetElementAsDate("date").ToSystemDateTime()
                              let bid = elmValues.GetElementAsFloat64("BID")
                              let ask = elmValues.GetElementAsFloat64("ASK")
                              
                              printfn "%s: BID = %f, ASK = %f" (date.ToShortDateString()) bid ask
                )

let HandleOtherEvent (eventObj : Event) =
    printfn "EventType = %s" (eventObj.Type.ToString())
    eventObj.GetMessages()
    |> Seq.iter (fun m -> printfn "correlationID=%A " m.CorrelationID
                          printfn "messageType=%A " m.MessageType
                          printfn "%A" m

                          if Event.EventType.SESSION_STATUS = eventObj.Type && m.MessageType.Equals("SessionTerminated") then
                              printfn "Terminating: %A" m.MessageType
                )

let RunExample() =
    let uri = "//blp/refdata"
    let operationName = "HistoricalDataRequest"
    let sessionOptions = new SessionOptions(ServerHost = "127.0.0.1", ServerPort = 8194)
    let session = new Session(sessionOptions)

    session.Start() |> ignore
    session.OpenService(uri) |> ignore

    let service = session.GetService(uri)
    let request = service.CreateRequest(operationName)

    request.Append("securities", "IBM US EQUITY")
    request.Append("securities", "SPY US EQUITY")
    request.Append("securities", "C A COMDTY")
    request.Append("securities", "AAPL 150117C00600000 EQUITY") //this is a stock option: TICKER yyMMdd[C/P]\d{8} EQUITY

    //include the following simple fields in the result
    request.Append("fields", "BID")
    request.Append("fields", "ASK")

    //Historical requests allow a few overrides.  See the developer's guide A.2.4 for more information.

    request.Set("startDate", DateTime.Today.AddMonths(-1).ToString("yyyyMMdd")) //Request that the information start three months ago from today.  This override is required.
    request.Set("endDate", DateTime.Today.AddDays(10.).ToString("yyyyMMdd")) //Request that the information end three days before today.  This is an optional override.  The default is today.
            
    //Determine the frequency and calendar type of the output. To be used in conjunction with Period Selection.
    request.Set("periodicityAdjustment", "CALENDAR") //Optional string.  Valid values are ACTUAL (default), CALENDAR, and FISCAL.

    //Determine the frequency of the output. To be used in conjunction with Period Adjustment.
    request.Set("periodicitySelection", "DAILY") //Optional string.  Valid values are DAILY (default), WEEKLY, MONTHLY, QUARTERLY, SEMI_ANNUALLY, and YEARLY

    //Sets quote to Price or Yield for a debt instrument whose default value is quoted in yield (depending on pricing source).
    request.Set("pricingOption", "PRICING_OPTION_PRICE") //Optional string.  Valid values are PRICING_OPTION_PRICE (default) and PRICING_OPTION_YIELD

    //Adjust for "change on day"
    request.Set("adjustmentNormal", true) //Optional bool. Valid values are true and false (default = false)

    //Adjusts for Anormal Cash Dividends
    request.Set("adjustmentAbnormal", false) //Optional bool. Valid values are true and false (default = false)

    //Capital Changes Defaults
    request.Set("adjustmentSplit", true) //Optional bool. Valid values are true and false (default = false)

    //The maximum number of data points to return, starting from the startDate
    //request.Set("maxDataPoints", 5) //Optional integer.  Valid values are positive integers.  The default is unspecified in which case the response will have all data points between startDate and endDate

    //Indicates whether to use the average or the closing price in quote calculation.
    request.Set("overrideOption", "OVERRIDE_OPTION_CLOSE") //Optional string.  Valid values are OVERRIDE_OPTION_GPA for an average and OVERRIDE_OPTION_CLOSE (default) for the closing price

    let requestID = new CorrelationID(1L)
    session.SendRequest(request, requestID) |> ignore

    let rec readResults() =
        let event = session.NextEvent()
        match event.Type with
        | Event.EventType.RESPONSE -> 
            HandleResponseEvent event
        | Event.EventType.PARTIAL_RESPONSE -> 
            HandleResponseEvent event
            readResults()
        | _ -> 
            HandleOtherEvent event
            readResults()

    readResults()
