module Server.MsgProcessor

open Server.StravaSync
open Shared

type SyncMessage = { owner: Owner; period: PeriodSelector }

let private createSyncQueue storage =
    MailboxProcessor.Start(fun inbox ->
        let rec messageLoop () =
            async {
                let! msg = inbox.Receive()
                do! sync msg.owner msg.period storage |> Async.AwaitTask
                return! messageLoop ()
            }

        messageLoop ())
type SyncQueue (storage) =
    let processor = createSyncQueue storage
    member __.Enqueue msg =
        processor.Post msg
