module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Model =
    { Todos: Todo list
      Input: string
      Doors: CalendarDoor list }

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let doors =
    [ { day = 1
        distance = 2.0
        opened = true
        finished = true }
      { day = 2
        distance = 2.0
        opened = true
        finished = true }
      { day = 3
        distance = 2.0
        opened = true
        finished = true }
      { day = 4
        distance = 2.0
        opened = true
        finished = true }
      { day = 5
        distance = 2.0
        opened = true
        finished = true }
      { day = 6
        distance = 2.0
        opened = true
        finished = true }
      { day = 7
        distance = 2.0
        opened = true
        finished = false }
      { day = 8
        distance = 2.0
        opened = true
        finished = false }
      { day = 9
        distance = 2.0
        opened = true
        finished = false }
      { day = 10
        distance = 2.0
        opened = false
        finished = false }
      { day = 11
        distance = 2.0
        opened = false
        finished = false }
      { day = 12
        distance = 2.0
        opened = false
        finished = false }
      { day = 13
        distance = 2.0
        opened = false
        finished = false }
      { day = 14
        distance = 2.0
        opened = false
        finished = false }
      { day = 15
        distance = 2.0
        opened = false
        finished = false }
      { day = 16
        distance = 2.0
        opened = false
        finished = false }
      { day = 17
        distance = 2.0
        opened = false
        finished = false }
      { day = 18
        distance = 2.0
        opened = false
        finished = false }
      { day = 19
        distance = 2.0
        opened = false
        finished = false }
      { day = 20
        distance = 2.0
        opened = false
        finished = false }
      { day = 21
        distance = 2.0
        opened = false
        finished = false }
      { day = 22
        distance = 2.0
        opened = false
        finished = false }
      { day = 23
        distance = 2.0
        opened = false
        finished = false }
      { day = 24
        distance = 2.0
        opened = false
        finished = false } ]

let init (): Model * Cmd<Msg> =
    let model =
        { Todos = []
          Input = ""
          Doors = doors }

    let cmd =
        Cmd.OfAsync.perform todosApi.getTodos () GotTodos

    model, cmd

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | GotTodos todos -> { model with Todos = todos }, Cmd.none
    | SetInput value -> { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input

        let cmd =
            Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

        { model with Input = "" }, cmd
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

let containerBox (model: Model) (dispatch: Msg -> unit) =
    Box.box' [] [
        Content.content [] [
            Content.Ol.ol [] [
                for todo in model.Todos do
                    li [] [ str todo.Description ]
            ]
        ]
        Field.div [ Field.IsGrouped ] [
            Control.p [ Control.IsExpanded ] [
                Input.text [ Input.Value model.Input
                             Input.Placeholder "What needs to be done?"
                             Input.OnChange(fun x -> SetInput x.Value |> dispatch) ]
            ]
            Control.p [] [
                Button.a [ Button.Color IsPrimary
                           Button.Disabled(Todo.isValid model.Input |> not)
                           Button.OnClick(fun _ -> dispatch AddTodo) ] [
                    str "Add"
                ]
            ]
        ]
    ]





let view (model: Model) (dispatch: Msg -> unit) =
    Hero.hero [ Hero.Color IsPrimary
                Hero.IsFullHeight
                Hero.Props
                    [ Style [ Background
                                  """linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url("https://unsplash.it/1200/900?random") no-repeat center center fixed"""
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
                                                JustifyContent "center"] ] ] [
                Heading.p [ Heading.Modifiers [ Modifier.TextAlignment(Screen.All, TextAlignment.Centered) ] ] [
                    str "Advent Run Ninja"
                ]
                Container.container [ Container.Props
                                          [ Style [ Display DisplayOptions.Flex
                                                    MaxWidth "1300px"
                                                    FlexDirection "row"
                                                    FlexWrap "wrap"
                                                    JustifyContent "center" ] ] ] [

                    for door in model.Doors do
                        Field.div [ Field.Props
                                        [ Style [ Height "150px"
                                                  Flex "0 0 150px"
                                                  Margin "20px"
                                                  Padding "10px"
                                                  BackgroundColor "grey" ] ] ] [
                            match door.opened with
                            | true -> str "true"
                            | false -> str "false"
//                            str (sprintf "%i" door.day)
                        ]
                ]
            ]
        ]
    ]
