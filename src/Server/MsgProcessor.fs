module Server.MsgProcessor

open Server.StravaSync
open Shared

type SyncMessage = { owner: Owner; period: PeriodSelector }

let private createSyncQueue storage logger =
    MailboxProcessor.Start(fun inbox ->
        let rec messageLoop () =
            async {
                let! msg = inbox.Receive()
                try
                    do! sync msg.owner msg.period storage logger |> Async.AwaitTask
                with
                | ex -> printfn $"Error processing message. {ex.Message}"
                return! messageLoop ()
            }

        messageLoop ())
type SyncQueue (storage, logger) =
    let processor = createSyncQueue storage logger
    member __.Enqueue msg =
        processor.Post msg
