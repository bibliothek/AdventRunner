module Index

open Elmish
open Fable.FontAwesome
open Fable.Remoting.Client
open Fulma
open Shared
open Elmish.Navigation
open Elmish.UrlParser
type Page =
    | Welcome
    | CalendarView of string
    | SettingsView of string

type Model =
    { Calendar: Calendar option
      Page : Page
      UserName : string
      IsBurgerOpen: bool }

type Msg =
    | MarkedDoorAsDone of CalendarDoor
    | MarkedDoorAsFailed of CalendarDoor
    | OpenDoor of CalendarDoor
    | Updated of Calendar
    | GotCalendar of Calendar
    | SetUserName of string
    | NavigateToCalendar
    | NavigateToSettings
    | SetDistanceFactor of double
    | Reset of string
    | ToggleBurger

let toHash =
    function
    | Welcome -> "#welcome"
    | CalendarView ownerName -> "#calendar/" + ownerName
    | SettingsView ownerName -> "#settings/" + ownerName

let pageParser : UrlParser.Parser<(Page -> Page),Page> =
  oneOf
    [ map Welcome (s "home")
      map CalendarView (s "calendar" </> str)
      map SettingsView (s "settings" </> str) ]

let adventRunApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IAdventRunApi>

let getCalendarCmd calendarView =
    Cmd.OfAsync.perform adventRunApi.getCalendar {name = calendarView} GotCalendar

let urlUpdate (result:Option<Page>) model =
  match result with
  | Some Welcome -> {model with Page = Welcome}, []
  | Some (CalendarView owner as page) ->
      { model with Page = page; UserName = owner }, getCalendarCmd owner
  | Some (SettingsView owner as page) ->
      { model with Page = page; UserName = owner }, getCalendarCmd owner
  | None ->
      ( model, Navigation.modifyUrl (toHash Welcome) )

let init result =
  urlUpdate result { Page = Welcome; Calendar = None; UserName = ""; IsBurgerOpen = false}

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
    | SetUserName ownerName ->
        { model with UserName = ownerName }, Cmd.none
    | SetDistanceFactor factor ->
        let newCalendar = Calendar.init { name = model.UserName } (Settings.init factor)
        let updatedModel = { model with Calendar = Some newCalendar }
        let cmd = Cmd.OfAsync.perform adventRunApi.updateCalendar updatedModel.Calendar.Value Updated
        updatedModel, cmd
    | NavigateToCalendar ->
        { model with Page = CalendarView model.UserName; IsBurgerOpen = false }, getCalendarCmd model.UserName
    | NavigateToSettings ->
        { model with Page = SettingsView model.UserName; IsBurgerOpen = false }, Navigation.modifyUrl (toHash (SettingsView model.UserName))
    | Reset owner ->
        let updatedModel = { model with Calendar = Some (Calendar.init { name = owner } Settings.initDefault) }
        let cmd = Cmd.OfAsync.perform adventRunApi.updateCalendar updatedModel.Calendar.Value Updated
        updatedModel, cmd
    | ToggleBurger -> {model with IsBurgerOpen = not model.IsBurgerOpen}, Cmd.none


open Fable.React
open Fable.React.Props

let welcomePageView (model : Model) (dispatch : Msg -> unit) =
    Box.box' [ Props [ Style [ MaxWidth "500px" ] ] ] [
        Field.div [ Field.IsGrouped ] [
            Control.p [ Control.IsExpanded ] [
                Input.text [
                  Input.Value model.UserName
                  Input.Placeholder "Please enter a name"
                  Input.OnChange (fun x -> SetUserName x.Value |> dispatch) ] ]
            Control.p [] [
                Button.a [
                    Button.Color IsPrimary
                    Button.Disabled (model.UserName.Length <= 0)
                    Button.OnClick (fun _ -> dispatch NavigateToCalendar) ]
                    [ str "Get started!" ] ]
        ]
    ]

let settingsPageView (model : Model) (dispatch : Msg -> unit) =
    let factor = model.Calendar.Value.settings.distanceFactor
    div[] [
        Heading.h1 [ Heading.IsSubtitle ] [
            str "Settings"
            br []
            Tag.tag [ Tag.Color IsDanger ]
                [ str "Any change will clear the progress!" ] ]
        Box.box'[] [
            Field.div []
                [ Label.label []
                    [ str "Distance" ]
                  Control.div []
                    [ Radio.radio
                        [ Props [ OnClick (fun _ -> SetDistanceFactor 0.5 |> dispatch) ] ]
                        [ Radio.input [
                            Radio.Input.Modifiers [ Modifier.Spacing (Spacing.MarginRight, Spacing.Is1) ]
                            Radio.Input.Name "distance"
                            Radio.Input.Props [ DefaultChecked (factor = 0.5) ] ]
                          str "Half" ]
                      Radio.radio
                        [ Props [ OnClick (fun _ -> SetDistanceFactor 1.0 |> dispatch) ] ]
                        [ Radio.input [
                            Radio.Input.Modifiers [ Modifier.Spacing (Spacing.MarginRight, Spacing.Is1) ]
                            Radio.Input.Name "distance"
                            Radio.Input.Props [ DefaultChecked (factor = 1.0) ] ]
                          str "Full" ]
                      Radio.radio
                        [ Props [ OnClick (fun _ -> SetDistanceFactor 2.0 |> dispatch) ] ]
                        [   Radio.input [
                                Radio.Input.Modifiers [ Modifier.Spacing (Spacing.MarginRight, Spacing.Is1) ]
                                Radio.Input.Name "distance"
                                Radio.Input.Props [ DefaultChecked (factor = 2.0) ] ]
                            str "Double" ] ] ]
            Field.div []
                [ Label.label []
                    [ str "As if you've done nothing" ]
                  Button.button
                    [   Button.OnClick (fun _ -> Reset model.UserName |> dispatch)
                        Button.Color IsDanger ]
                    [ str "Reset" ] ]
        ]
    ]


let navbar isBurgerOpen dispatch =
    Navbar.navbar [] [
        Navbar.Brand.div [] [
            Navbar.burger [ Navbar.Burger.OnClick (fun _ -> ToggleBurger |> dispatch) ] [
                span[][]
                span[][]
                span[][] ] ]
        Navbar.menu [ Navbar.Menu.IsActive isBurgerOpen ] [
            Navbar.Start.div [] [
                Navbar.Link.div [
                    Navbar.Link.IsArrowless
                    Navbar.Link.Props [ OnClick(fun _ -> NavigateToCalendar |> dispatch) ]
                ] [ str "Home" ]
                Navbar.Link.div [
                    Navbar.Link.IsArrowless
                    Navbar.Link.Props [ OnClick(fun _ -> NavigateToSettings |> dispatch) ]
                ] [ str "Settings" ] ] ] ]

let toLevelItem (caption: string, doors: CalendarDoor list) =
    let cntDistanceFor doors = doors |> List.sumBy (fun x -> x.distance)
    let title = sprintf "%.1f km" (cntDistanceFor doors)
    Level.item [ Level.Item.HasTextCentered ]
            [ div []
                [ Level.heading [] [ str caption ]
                  Level.title [] [ str title ] ] ]

let completionStatsView (calendar: Calendar) =
    let filterFor state = calendar.doors |> List.filter (fun x -> x.state = state)
    let dones = filterFor Done
    let failed = filterFor Failed
    let left = calendar.doors |> List.except dones |> List.except failed
    let doors =
        [ ("Left", left); ("Done", dones); ("Failed", failed) ]
        |> List.filter (fun x -> snd x |> List.isEmpty |> not)
    Level.level [] (doors |> List.map toLevelItem)

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
                    [ Heading.h2 []
                        [ str (sprintf "%i" door.day) ] ]

let doorActionsView door dispatch =
    match door.state with
    | DoorState.Open ->
        [   div [ Style [ FlexGrow "1" ] ] []
            Button.list [] [
                Button.button [
                Button.Color IsSuccess
                Button.IsOutlined
                Button.OnClick(fun _ -> MarkedDoorAsDone door |> dispatch) ]
                    [ Icon.icon []
                        [ Fa.i [ Fa.Solid.Check ]
                        [] ] ]
                Button.button [
                    Button.Color IsDanger
                    Button.IsOutlined
                    Button.OnClick(fun _ -> MarkedDoorAsFailed door |> dispatch) ]
                        [ Icon.icon []
                            [ Fa.i [ Fa.Solid.Times ] [] ] ]
                        ]
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
            [ str (sprintf "%.1f km" door.distance) ]
        div [ Style [ FlexGrow "1" ] ] []
        div [ Style [ Display DisplayOptions.Flex
                      FlexDirection "row"
                      AlignItems AlignItemsOptions.Baseline ] ]
            (doorActionsView door dispatch)
    ]

let doorView door dispatch =
    div [ Style [ Height "15rem"
                  Width "15rem"
                  Margin "auto" ] ] [
        match door.state with
        | Closed -> closedDoorView door dispatch
        | _ -> openedDoorView door dispatch
    ]

let titleView =
    Heading.h1
      [ Heading.Modifiers [] ]
      [ str "Advent Runner" ]

let calendarPageView model dispatch =
    div [] [
        match model.Calendar with
            | Some c -> completionStatsView c
            | _ -> div [] []
        match model.Calendar with
        | Some cal ->
            Container.container [] [
                Columns.columns [ Columns.IsMultiline ] [
                    for door in cal.doors do
                        Column.column
                            [ Column.Width (Screen.Touch, Column.IsOneThird) ]
                            [ doorView door dispatch ] ]
                    ]
        | None -> str "Loading"
    ]

let viewForPage model dispatch =
    match model.Page with
    | Welcome ->
        [   titleView
            welcomePageView model dispatch ]
    | SettingsView _ ->
        [ settingsPageView model dispatch ]
    | CalendarView _ ->
        [   titleView
            calendarPageView model dispatch ]

let view (model: Model) (dispatch: Msg -> unit) =
    Hero.hero [ Hero.Color IsPrimary
                Hero.IsFullHeight
                Hero.Props
                    [ Style [ Background
                                  """linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url("https://picsum.photos/id/15/1200/900?random") no-repeat center center fixed"""
                              BackgroundSize "cover" ] ] ] [
        Hero.head [] [
            if model.UserName <> "" then
                (navbar model.IsBurgerOpen dispatch)
        ]
        Hero.body [] [
            Container.container
                [ Container.Props
                      [ Style [ Display DisplayOptions.Flex
                                FlexDirection "column"
                                JustifyContent "center"
                                AlignItems AlignItemsOptions.Center ] ] ]
                (viewForPage model dispatch)
        ]
    ]
