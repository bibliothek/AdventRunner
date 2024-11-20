module Server.Migrations.Migration20240222CreateTables

open FluentMigrator

[<Migration(20240222L)>]
type Migration20240222CreateTables() =
    inherit Migration()

    override this.Up() =
        this.Create
            .Table("UserData")
            .WithColumn("id")
            .AsString()
            .WithColumn("data")
            .AsString()
        |> ignore

    override this.Down() = this.Delete.Table("UserData") |> ignore
