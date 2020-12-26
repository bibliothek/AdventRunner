module Index

open Elmish
open Fable.FontAwesome
open Fable.Remoting.Client
open Fulma
open Shared
open Elmish.Navigation
open Elmish.UrlParser
type Page = Welcome | CalendarView of string

type Model =
    { Calendar: Calendar option
      Page : Page
      OwnerName : string }

type Msg =
    | MarkedDoorAsDone of CalendarDoor
    | MarkedDoorAsFailed of CalendarDoor
    | OpenDoor of CalendarDoor
    | Updated of Calendar
    | GotCalendar of Calendar
    | SetOwnerName of string
    | NavigateToCalendar
    | Restart of string

let toHash =
    function
    | Welcome -> "#welcome"
    | CalendarView ownerName -> "#calendar/" + ownerName

let pageParser : UrlParser.Parser<(Page -> Page),Page> =
  oneOf
    [ map Welcome (s "home")
      map CalendarView (s "calendar" </> str) ]

let adventRunApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IAdventRunApi>

let getCalendarCmd calendarView =
    Cmd.OfAsync.perform adventRunApi.getCalendar {name = calendarView} GotCalendar

let urlUpdate (result:Option<Page>) model =
  match result with
  | Some Welcome -> {model with Page = Welcome}, []
  | Some (CalendarView calView as page) ->
      { model with Page = page},
        getCalendarCmd calView
  | None ->
      ( model, Navigation.modifyUrl (toHash Welcome) )

let init result =
  urlUpdate result { Page = Welcome; Calendar = None; OwnerName = ""}

let updateDoor model door =
    match model.Calendar with
        | Some cal ->
            let newDoors =
                cal.doors.GetSlice(None, Some(door.day - 2))
                @ [ door ]
                @ cal.doors.GetSlice(Some(door.day), None)
            let newCalendar = { cal with doors = newDoors }
            { model with Calendar = Some newCalendar }
        | _ -> failwith "should not happen"

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | MarkedDoorAsDone door ->
        let updatedDoor = { door with state = Done }
        let updatedModel = updateDoor model updatedDoor
        let cmd = Cmd.OfAsync.perform adventRunApi.updateCalendar updatedModel.Calendar.Value Updated
        updatedModel, cmd
    | MarkedDoorAsFailed door ->
        let updatedDoor = { door with state = Failed }
        let updatedModel = updateDoor model updatedDoor
        let cmd = Cmd.OfAsync.perform adventRunApi.updateCalendar updatedModel.Calendar.Value Updated
        updatedModel, cmd
    | OpenDoor door ->
        let updatedDoor = { door with state = Open }
        let updatedModel = updateDoor model updatedDoor
        let cmd = Cmd.OfAsync.perform adventRunApi.updateCalendar updatedModel.Calendar.Value Updated
        updatedModel, cmd
    | Updated _ ->
        model, Cmd.none
    | GotCalendar calendar ->
        {model with Calendar = Some calendar}, Navigation.modifyUrl (toHash (CalendarView calendar.owner.name))
    | SetOwnerName ownerName ->
        {model with OwnerName = ownerName}, Cmd.none
    | NavigateToCalendar ->
        {model with Page = CalendarView model.OwnerName}, getCalendarCmd model.OwnerName
    | Restart owner ->
        let updatedModel = { model with Calendar = Some (Calendar.init { name = owner }) }
        let cmd = Cmd.OfAsync.perform adventRunApi.updateCalendar updatedModel.Calendar.Value Updated
        updatedModel, cmd


open Fable.React
open Fable.React.Props

let welcomeView (model : Model) (dispatch : Msg -> unit) =
    Box.box' [ ] [
        Field.div [ Field.IsGrouped ] [
            Control.p [ Control.IsExpanded ] [
                Input.text [
                  Input.Value model.OwnerName
                  Input.Placeholder "Please enter a name"
                  Input.OnChange (fun x -> SetOwnerName x.Value |> dispatch) ]
            ]
            Control.p [ ] [
                Button.a [
                    Button.Color IsPrimary
                    Button.Disabled (model.OwnerName.Length <= 0)
                    Button.OnClick (fun _ -> dispatch NavigateToCalendar)
                ] [
                    str "Get started!"
                ]
            ]
        ]
    ]

let navBrand =
    Navbar.Brand.div [] [
        Navbar.Item.a [ Navbar.Item.Props [ Href "https://safe-stack.github.io/" ]
                        Navbar.Item.IsActive true ] [
            img [ Src "/safe.png"; Alt "Logo" ]
        ]
    ]

let toLevelItem (caption: string, doors: CalendarDoor list) =
    let cntDistanceFor doors = doors |> List.sumBy (fun x -> x.distance)
    let title = sprintf "%i km" (cntDistanceFor doors)
    Level.item [ Level.Item.HasTextCentered ]
            [ div [ ]
                [ Level.heading [ ]
                    [ str caption ]
                  Level.title [ ]
                    [ str title ] ] ]

let completionStatsView (calendar: Calendar) =
    let filterFor state = calendar.doors |> List.filter (fun x -> x.state = state)
    let dones = filterFor Done
    let failed = filterFor Failed
    let left = calendar.doors |> List.except dones |> List.except failed
    let doors =
        [ ("Left", left); ("Done", dones); ("Failed", failed) ]
        |> List.filter (fun x -> snd x |> List.isEmpty |> not)
    Level.level [ ] (doors |> List.map toLevelItem)

let closedDoorView door dispatch =
    Button.button [ Button.OnClick(fun _ -> OpenDoor door |> dispatch)
                    Button.Color IsPrimary
                    Button.Props [
                        Style [   Height "100%"
                                  Width "100%"
                                  Display DisplayOptions.Flex
                                  FlexDirection "column"
                                  JustifyContent "center"
                                  AlignItems AlignItemsOptions.Center
                                  AlignContent AlignContentOptions.Center
                                  Border "5px solid #C6C6C6"
                                  BorderRadius "5px"
                                   ] ] ]
                    [ Heading.h2 [ ]
                        [ str (sprintf "%i" door.day) ] ]

let doorActionsView door dispatch =
    match door.state with
    | DoorState.Open ->
        [   div [ Style [ FlexGrow "1" ] ] []
            Button.button [
            Button.Color IsSuccess
            Button.IsOutlined
            Button.Size IsSmall
            Button.OnClick(fun _ -> MarkedDoorAsDone door |> dispatch) ]
                [ Icon.icon [ ]
                    [ Fa.i [ Fa.Solid.Check ]
                    [ ] ] ]
            Button.button [
                Button.Modifiers [ Modifier.Spacing (Spacing.MarginLeft, Spacing.Is2) ]
                Button.Color IsDanger
                Button.IsOutlined
                Button.Size IsSmall
                Button.OnClick(fun _ -> MarkedDoorAsFailed door |> dispatch) ]
                    [ Icon.icon [ ]
                        [ Fa.i [ Fa.Solid.Times ] [ ] ] ]
                    ]
    | Done ->
        [ div [ Style [ FlexGrow "1" ] ] []
          Tag.tag [ Tag.Color IsSuccess ] [ str "done" ] ]
    | Failed ->
        [ div [ Style [ FlexGrow "1" ] ] []
          Tag.tag [ Tag.Color IsDanger ] [ str "failed" ] ]
    | Closed -> failwith "how dare you"

let classNameFor (door: CalendarDoor) =
    match door.state with
    | Done -> "door done"
    | Failed -> "door failed"
    | _ -> "door"

let openedDoorView door dispatch =
    div [ ClassName (classNameFor door)
          Style [ Height "100%"
                  Color "black"
                  Display DisplayOptions.Flex
                  FlexDirection "column"
                  Padding "5px 10px"
                  BackgroundColor "#C6C6C6" ] ] [
        p [] [ str (sprintf "%i" door.day) ]
        div [ Style [ FlexGrow "1" ] ] []
        Heading.h2 [
            Heading.Modifiers [ Modifier.TextColor IsPrimary ]
            Heading.Option.Props [
                Style [
                    TextAlign TextAlignOptions.Center
                    MarginBottom 0
        ] ] ]
            [ str (sprintf "%i km" door.distance) ]
        div [ Style [ FlexGrow "1" ] ] []
        div [ Style [ Display DisplayOptions.Flex
                      FlexDirection "row"
                      AlignItems AlignItemsOptions.Baseline ] ]
            (doorActionsView door dispatch)
    ]

let doorView door dispatch =
    div [ Style [ Height "180px"
                  Flex "0 0 180px"
                  Margin "10px"
                  Padding "2px" ] ] [
        match door.state with
        | Closed -> closedDoorView door dispatch
        | _ -> openedDoorView door dispatch
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Hero.hero [ Hero.Color IsPrimary
                Hero.IsFullHeight
                Hero.Props
                    [ Style [ Background
                                  """linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url("https://picsum.photos/id/15/1200/900?random") no-repeat center center fixed"""
                              BackgroundSize "cover" ] ] ] [
        Hero.head [] [
            Navbar.navbar [] [
                Container.container [] [ navBrand ]
            ]
        ]

        Hero.body [] [
            Container.container [ Container.Props
                                      [ Style [ Display DisplayOptions.Flex
                                                FlexDirection "column"
                                                JustifyContent "center" ] ] ] [
                Heading.h1 [ Heading.Modifiers [ Modifier.TextAlignment(Screen.All, TextAlignment.Centered)
                                                 Modifier.TextSize(Screen.All, TextSize.Is1) ] ] [
                    str "Advent Runner"
                ]
                match model.Calendar with
                | Some c -> completionStatsView c
                | _ -> div [] []

                Container.container [ Container.Props
                                          [ Style [ Display DisplayOptions.Flex
                                                    MaxWidth "1300px"
                                                    FlexDirection "row"
                                                    FlexWrap "wrap"
                                                    JustifyContent "center" ] ] ] [
                    match model.Page with
                    | Welcome -> welcomeView model dispatch
                    | CalendarView _ ->
                        match model.Calendar with
                        | Some cal ->
                            for door in cal.doors do
                                doorView door dispatch
                        | None -> str "Loading"
                ]
            ]
        ]
    ]
