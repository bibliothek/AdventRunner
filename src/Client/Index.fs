module Index

open Elmish
open Elmish
open Fable.Remoting.Client
open Shared

type Model =
    { Todos: Todo list
      Input: string
      Calendar: Calendar option }

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo
    | MarkedDoorAsDone of CalendarDoor
    | OpenDoor of CalendarDoor
    | Updated of Calendar
    | GotCalendar of Calendar

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let adventRunApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IAdventRunApi>


let init (): Model * Cmd<Msg> =
    let model =
        { Todos = []
          Input = ""
          Calendar = None }

    let cmd =
        Cmd.OfAsync.perform adventRunApi.getCalendar {name = "matha"} GotCalendar

    model, cmd

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
    | GotTodos todos -> { model with Todos = todos }, Cmd.none
    | SetInput value -> { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input

        let cmd =
            Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

        { model with Input = "" }, cmd
    | Updated _ ->
        model, Cmd.none
    | GotCalendar calendar ->
        {model with Calendar = Some calendar}, Cmd.none
    | AddedTodo todo ->
        { model with
              Todos = model.Todos @ [ todo ] },
        Cmd.none

open Fable.React
open Fable.React.Props
open Fulma

let navBrand =
    Navbar.Brand.div [] [
        Navbar.Item.a [ Navbar.Item.Props [ Href "https://safe-stack.github.io/" ]
                        Navbar.Item.IsActive true ] [
            img [ Src "/favicon.png"; Alt "Logo" ]
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
                    str "Advent Run Ninja"
                ]
                Container.container [ Container.Props
                                          [ Style [ Display DisplayOptions.Flex
                                                    MaxWidth "1300px"
                                                    FlexDirection "row"
                                                    FlexWrap "wrap"
                                                    JustifyContent "center" ] ] ] [
                    match model.Calendar with
                    | None -> str "Loading..."
                    | Some cal ->
                        for door in cal.doors do
                            doorView door dispatch
                ]
            ]
        ]
    ]
