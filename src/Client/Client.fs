module Client

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Fetch.Types
open Thoth.Fetch
open Fulma
open Thoth.Json
open Feliz
open Shared
open System

type Day =
    { Number: int }

type Model =
    {
      Month: int
      Year: int
      SelectedDay: System.DateTime option
      CurrentEvent: Components.Events.CalendarEvent option
      Events: Components.Events.CalendarEvent List
      EventAddingInProgress: bool
    }

type Msg =
    | MonthChange    of int
    | MonthNow
    | EventAdd       of System.DateTime
    | EventSave      of Components.Events.CalendarEvent
    | EventAddToggle of bool * Components.Events.CalendarEvent option

let init(): Model * Cmd<Msg> =
    let initialModel =
        {
          Month = System.DateTime.Today.Month
          Year = System.DateTime.Today.Year
          SelectedDay = None
          CurrentEvent = None
          Events = []
          EventAddingInProgress = false
        }

    initialModel, Cmd.none

type DialogProps = {
  cancel:   unit -> unit
  save:     Components.Events.CalendarEvent -> unit
  currentEvent: Components.Events.CalendarEvent option
  }

let addEventModal = React.functionComponent("modalEvent", fun (props: DialogProps) ->
    // Some doc: https://zaid-ajaj.github.io/Feliz/#/Feliz/React/SubscriptionsWithEffects
    let eltRef = React.useElementRef()
    let (currentEvent, setEventDetails) = React.useState( match props.currentEvent with
                                                              | Some (evt) -> evt
                                                              | None -> {Name= ""; Details="" } )

    let modalKey (ev:Browser.Types.Event) =
        let kev = ev :?> Browser.Types.KeyboardEvent
        if kev.keyCode = 27. then
          props.cancel()

    let windowsKeyDownEffect () =
      Browser.Dom.window.addEventListener("keydown", modalKey)
      { new IDisposable with member this.Dispose() =
                              Browser.Dom.window.removeEventListener("keydown", modalKey)
                             }

    React.useEffect(windowsKeyDownEffect)

    Html.div [
      Html.div [
        prop.className [true, "modal"; true, "is-active"]
        prop.children [
          Html.div [
            prop.onClick (fun _ -> props.cancel() )
            prop.classes ["modal-background"] ]
          // Mix with Fulma: https://fulma.github.io/Fulma/#fulma/components/modal
          Modal.Card.card [ ]
            [ Modal.Card.head [ ]
                [ Modal.Card.title [ ]
                    [ str "Add Event" ]
                  // Close button
                  Delete.delete [ Delete.OnClick <| fun _ -> props.cancel() ] [ ] ]
              Modal.Card.body [ ]
                [
                  Components.Events.Add {
                    handleChanges = (fun e -> setEventDetails e )
                    current = currentEvent
                    save = (fun _ -> props.save currentEvent)
                  }
                ]
              Modal.Card.foot [ ]
                [ Button.button [
                    Button.Color IsSuccess
                    Button.OnClick <| fun e -> props.save currentEvent
                    ]
                    [ str "Save Event" ]
                  Button.button [ Button.OnClick <| fun _ -> props.cancel() ]
                    [ str "Cancel" ] ] ]
        ] ]
      ]
    )

let update (msg: Msg) (state: Model): Model * Cmd<Msg> =
    match msg with
    | MonthChange nbMonths ->
        let date = System.DateTime(state.Year, state.Month, 1)
        let newDate = date.AddMonths(nbMonths)
        let nextState = { state with Month = newDate.Month; Year = newDate.Year }
        nextState, Cmd.none
    | MonthNow ->
        let nextState = { state with
                            Month = System.DateTime.Now.Month
                            Year = System.DateTime.Now.Year
                        }
        nextState, Cmd.none
    | EventAdd day ->
        printfn "Day: %A" day
        state, Cmd.ofMsg (EventAddToggle (true, None))
    | EventSave e ->
        let nextState = { state with Events = List.append state.Events [e] }
        nextState, Cmd.ofMsg (EventAddToggle (false, None))
    | EventAddToggle (visible, e) ->
        let nextState = { state with EventAddingInProgress = visible; CurrentEvent = e }
        nextState, Cmd.none

let safeComponents =
    Html.span [
        str "Version "
        Html.strong [ str Version.app ]
        str " powered by: Someone lost ;)" ]

let getMonthName (month: int) =
  let months = [|"January";"February";"March";"April";"May";"June";"July";"August";"September";"October";"November";"December"|]
  months.[month-1]

let getDayOfWeekName (day: System.DayOfWeek) =
  match day with
    | System.DayOfWeek.Monday     -> "Mon"
    | System.DayOfWeek.Tuesday    -> "Tue"
    | System.DayOfWeek.Wednesday  -> "Wed"
    | System.DayOfWeek.Thursday   -> "Thu"
    | System.DayOfWeek.Friday     -> "Fri"
    | System.DayOfWeek.Saturday   -> "Sat"
    | System.DayOfWeek.Sunday     -> "Sun"
    | _                           -> "Should never happen"

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ] [ str txt ]

let drawCalendar (model: Model) (dispatch: Msg -> unit) =
    Html.div [
      prop.classes [ "calendar"; "is-calendar-large" ]
      prop.children [

        // Top bar
        Html.div [
          prop.classes ["calendar-nav"]
          prop.children [
            Html.div [
              prop.classes ["calendar-nav-left"]
              prop.children [
                Html.button [
                  prop.classes ["button"; "is-warning"; "is-light"]
                  prop.onClick (fun _ -> dispatch (MonthChange -1))
                  prop.children [
                    Html.i [
                      prop.classes ["fa fa-chevron-left"] ] ] ] ] ]

            Html.div [
              Html.span [ prop.text (sprintf "%s %i " (getMonthName model.Month) model.Year)]
              if model.Month <> System.DateTime.Today.Month || model.Year <> System.DateTime.Today.Year then
                Html.button [
                  prop.classes ["button"; "is-info"; "is-light"]
                  prop.onClick (fun _ -> dispatch (MonthNow))
                  prop.text "Today"
                ]
              ]

            Html.div [
              prop.classes ["calendar-nav-right"]
              prop.children [
                Html.button [
                  prop.classes ["button"; "is-warning"; "is-light"]
                  prop.onClick (fun _ -> dispatch (MonthChange 1))
                  prop.children [
                    Html.i [
                      prop.classes ["fa fa-chevron-right"] ] ] ] ] ] ] ]

        // Day header
        Html.div [
          prop.classes ["calendar-container"]
          prop.children [
            Html.div [
              prop.classes ["calendar-header"]
              // Loop through day of weeks
              prop.children [
                for dow in 0..6 do
                  Html.div [
                    prop.className ["calendar-date"]
                    prop.text (sprintf "%s" (getDayOfWeekName (enum<System.DayOfWeek>(dow))))
                  ]
              ]
            ] ] ]

        // Main calendar part
        Html.div [
          prop.classes ["calendar-body"]
          prop.children [
            let first = System.DateTime (model.Year, model.Month, 1)
            let firstDayInCalendar = first.AddDays(- ((float)first.DayOfWeek))
            let isToday (day:System.DateTime) =
              day.Year = System.DateTime.Today.Year && day.Month = System.DateTime.Today.Month && day.Day = System.DateTime.Today.Day
            // If first day of week is not sunday, it's 35... otherwise it's 30 when less than 31 day
            for d in 0.0..34.0 do // 7x5 ... so 35 days in the current calendar
              let currentDay = firstDayInCalendar.AddDays(d)
              Html.div [
                prop.className [true, "calendar-date"; currentDay.Month <> model.Month, "is-disabled"]
                prop.children [
                  Html.button [
                    prop.className [true, "date-item"; (isToday currentDay), "is-today"]
                    prop.text (sprintf "%i" currentDay.Day )
                    prop.onClick (fun _ -> dispatch (EventAdd currentDay) )
                  ]
                ]
              ]
          ] ]

        // Control elments of the UI
        div [ ]
          [
            // basicModal model.EventAddModalVisible (fun _ -> dispatch (EventAddModal false))
            if model.EventAddingInProgress then
              addEventModal {
                currentEvent = model.CurrentEvent
                cancel = (fun () ->
                              dispatch <| EventAddToggle (false, None))
                save = (fun (e: Components.Events.CalendarEvent) ->
                              dispatch <| EventSave e
                      )
                }
          ]
    ] ]

let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
      Navbar.navbar [ Navbar.Color IsPrimary] [ Navbar.Item.div [] [ Html.h2 [
        prop.className ["has-text-white"]
        prop.text "My Calendar is SAFE" ] ] ]

      Html.section [
        prop.className ["section"]
        prop.children [
            drawCalendar model dispatch
        ] ]

      Html.footer [ Content.content [ Content.Modifiers [ Modifier.TextAlignment(Screen.All, TextAlignment.Centered) ] ] [ safeComponents ] ]

     ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-calendar"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
