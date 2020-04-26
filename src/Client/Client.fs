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
    | MonthViewChange    of int
    | MonthViewToday
    | EventAdd       of System.DateTime
    | EventSave      of Components.Events.CalendarEvent
    | EventEditToggle of bool * Components.Events.CalendarEvent option

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
  currentEvent: Components.Events.CalendarEvent
  }

let addEventModal = React.functionComponent("modalEvent", fun (props: DialogProps) ->
    // Some doc: https://zaid-ajaj.github.io/Feliz/#/Feliz/React/SubscriptionsWithEffects
    let eltRef = React.useElementRef()
    let (currentEvent, setEventDetails) = React.useState( props.currentEvent )

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
    match state, msg with
    | _, MonthViewChange nbMonths ->
        let date = System.DateTime(state.Year, state.Month, 1)
        let newDate = date.AddMonths(nbMonths)
        let nextState = { state with Month = newDate.Month; Year = newDate.Year }
        nextState, Cmd.none
    | _, MonthViewToday ->
        let nextState = { state with
                            Month = System.DateTime.Now.Month
                            Year = System.DateTime.Now.Year
                        }
        nextState, Cmd.none
    | _, EventAdd day ->
        state, Cmd.ofMsg (EventEditToggle (true, Some ({ Id = -1; Day = day; Name= ""; Details=""; })))
    | _, EventSave e ->
        printf "Eventid : %i" e.Id
        let eventToSave =
          if e.Id = -1 then
            let newId = match state.Events with
                          | [ ] -> 1
                          | e ->
                              e
                              |> List.maxBy (fun evt -> evt.Id)
                              |> fun evt -> evt.Id + 1
            List.append state.Events [{ e with Id = newId }]
          else
            state.Events
              |> List.map (fun evt ->
                              if evt.Id = e.Id then
                                e
                              else
                                evt
                              )

        // printf "NewEventid : %i" eventToSave.Id

        let nextState = { state with Events = eventToSave }
        // let nextState = { state with Events = List.append state.Events [eventToSave] }
        nextState, Cmd.ofMsg (EventEditToggle (false, None))
    | _, EventEditToggle (visible, e) ->
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
                  prop.onClick (fun _ -> dispatch (MonthViewChange -1))
                  prop.children [
                    Html.i [
                      prop.classes ["fa fa-chevron-left"] ] ] ] ] ]

            Html.div [
              Html.span [ prop.text (sprintf "%s %i " (getMonthName model.Month) model.Year)]
              if model.Month <> System.DateTime.Today.Month || model.Year <> System.DateTime.Today.Year then
                Html.button [
                  prop.classes ["button"; "is-info"; "is-light"]
                  prop.onClick (fun _ -> dispatch (MonthViewToday))
                  prop.text "Today"
                ]
              ]

            Html.div [
              prop.classes ["calendar-nav-right"]
              prop.children [
                Html.button [
                  prop.classes ["button"; "is-warning"; "is-light"]
                  prop.onClick (fun _ -> dispatch (MonthViewChange 1))
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
                    prop.text (sprintf "%s" (CalendarUtil.getDayOfWeekName (enum<System.DayOfWeek>(dow))))
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
            let isTooltips (day:System.DateTime) =
              model.Events |> List.exists (fun e -> e.Day = day)
            for d in 0.0..34.0 do // 7x5 ... so 35 days in the current calendar
              let currentDay = firstDayInCalendar.AddDays(d)
              Html.div [
                prop.className [true, "calendar-date"; currentDay.Month <> model.Month, "is-disabled"; (isTooltips currentDay), "tooltips"]
                if isTooltips currentDay then
                  Interop.mkAttr "data-tooltip" "You have appointment"
                prop.children [
                  Html.button [
                    prop.className [true, "date-item"; (isToday currentDay), "is-today"]
                    prop.text (sprintf "%i" currentDay.Day )
                    prop.onClick (fun _ -> dispatch (EventAdd currentDay) )
                  ]
                  Html.div [
                    prop.className "calendar-events"
                    prop.children [
                      let todayEvents = model.Events |> List.filter (fun e -> e.Day = currentDay)
                      for ev in todayEvents do
                        Html.a [
                          prop.className ["calendar-event"; "is-primary"]
                          prop.text ev.Name
                          prop.onClick (fun _ -> dispatch <| EventEditToggle (true, Some(ev)) )
                        ]
                    ]
                  ]
                ]
              ]
          ] ]

        // Control elments of the UI
        div [ ]
          [
            if model.CurrentEvent.IsSome && model.EventAddingInProgress then
              addEventModal {
                currentEvent = model.CurrentEvent.Value
                cancel = (fun () -> dispatch <| EventEditToggle (false, None))
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
