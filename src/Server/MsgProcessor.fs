module Server.MsgProcessor

open Shared

type SyncMessage = { owner: Owner }

let private createSyncQueue storage =
    MailboxProcessor.Start(fun inbox ->
        let rec messageLoop () =
            async {
                let! msg = inbox.Receive()
                do! StravaSync.sync msg.owner storage |> Async.AwaitTask
                return! messageLoop ()
            }

        messageLoop ())
type SyncQueue (storage) =
    let processor = createSyncQueue storage
    member __.Enqueue msg =
        processor.Post msg
