module Index

open Elmish
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
    | OpenDoor of CalendarDoor
    | Updated of Calendar
    | GotCalendar of Calendar
    | SetOwnerName of string
    | NavigateToCalendar

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
        let updatedDoor = { door with finished = true }
        let updatedModel = updateDoor model updatedDoor
        let cmd = Cmd.OfAsync.perform adventRunApi.updateCalendar updatedModel.Calendar.Value Updated
        updatedModel, cmd
    | OpenDoor door ->
        let updatedDoor = { door with opened = true }
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

open Fable.React
open Fable.React.Props
open Fulma

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

let closedDoorView door dispatch =
    div [ OnClick(fun _ -> OpenDoor door |> dispatch)
          Style [ Height "100%"
                  Display DisplayOptions.Flex
                  FlexDirection "column"
                  JustifyContent "center"
                  AlignItems AlignItemsOptions.Center
                  AlignContent AlignContentOptions.Center
                  BackgroundColor "#082510"
                  Border "5px solid #C6C6C6"
                  BorderRadius "5px" ] ] [
        p [ Style [ FontSize "2em" ] ] [
            str (sprintf "%i" door.day)
        ]
    ]

let openedDoorView door dispatch =
    div [ Style [ Height "100%"
                  Color "black"
                  Display DisplayOptions.Flex
                  FlexDirection "column"
                  Padding "5px 10px"
                  BackgroundColor "#C6C6C6"
                  Border "5px solid #082510"
                  BorderRadius "5px" ] ] [
        p [] [ str (sprintf "%i" door.day) ]
        div [ Style [ FlexGrow "1" ] ] []
        p [ Style [ TextAlign TextAlignOptions.Center
                    FontSize "2em" ] ] [
            str (sprintf "%i km" door.distance)
        ]
        div [ Style [ FlexGrow "1" ] ] []
        div [ Style [ Display DisplayOptions.Flex
                      FlexDirection "row"
                      AlignItems AlignItemsOptions.Baseline ] ] [
            div [ Style [ Color "#082510" ] ] [
                match door.finished with
                | true -> Icon.icon [ Icon.CustomClass "fas fa-check fa-2x" ] []
                | false -> Icon.icon [ Icon.CustomClass "fas fa-running fa-2x" ] []
            ]
            div [ Style [ FlexGrow "1" ] ] []
            if not door.finished then
                Button.a [ Button.Color IsPrimary
                           Button.OnClick(fun _ -> MarkedDoorAsDone door |> dispatch) ] [
                    str "Done!"
                ]
        ]
    ]

let doorView door dispatch =
    div [ Style [ Height "180px"
                  Flex "0 0 180px"
                  Margin "10px"
                  Padding "2px" ] ] [
        match door.opened with
        | true -> openedDoorView door dispatch
        | false -> closedDoorView door dispatch
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
                Heading.p [ Heading.Modifiers [ Modifier.TextAlignment(Screen.All, TextAlignment.Centered)
                                                Modifier.TextSize(Screen.All, TextSize.Is1) ] ] [
                    str "Advent Runner"
                ]
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
