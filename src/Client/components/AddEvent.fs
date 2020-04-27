module Components.Events

open Elmish
open Feliz
open Fable.React
open Fable.React.Props
open Fulma
open Shared

type EventsActions = {
  current: CalendarEvent
  handleChanges: CalendarEvent -> unit
  save: unit -> unit
}

let Add = React.functionComponent("FormAddEvent", fun (events:EventsActions) ->

  form [
    Props.OnSubmit (fun e ->
          e.preventDefault()
          events.save ()
        ) ]
   [ // Name field
     Field.div [ ]
          [ Label.label [ ]
              [ str "Title" ]
            Control.div [ ]
              [ Input.text [
                  Input.Placeholder "Ex: Going to party"
                  Input.Value events.current.Name
                  Input.OnChange (fun e -> events.handleChanges ({ events.current with Name = e.Value })) ] ] ]

     // Detail field
     Field.div [ ]
          [ Label.label [ ]
              [ str "Details" ]
            Control.div [
                // Control.IsLoading true
              ]
              [ Textarea.textarea [
                    Textarea.Value events.current.Details
                    Textarea.OnChange
                      <| fun e -> events.handleChanges
                                  <| { events.current with Details = e.Value }
                  ]
                  [ ] ] ]
    ]

    //  // Email field
    //  Field.div [ ]
    //       [ Label.label [ ]
    //           [ str "Email" ]
    //         Control.div [ Control.HasIconLeft
    //                       Control.HasIconRight ]
    //           [ Input.email [ Input.Color IsDanger
    //                           Input.DefaultValue "hello@" ]
    //             Icon.icon [ Icon.Size IsSmall; Icon.IsLeft ]
    //               [  ]
    //             Icon.icon [ Icon.Size IsSmall; Icon.IsRight ]
    //               [  ] ]
    //         Help.help [ Help.Color IsDanger ]
    //           [ str "This email is invalid" ] ]

    //  // Phone field
    //  Field.div [ ]
    //       [ Field.div [ Field.HasAddons ]
    //           [ Control.p [ ]
    //               [ Button.button [ Button.IsStatic true ]
    //                   [ str "+32" ] ]
    //             Control.p [ Control.IsExpanded ]
    //               [ Input.tel [ Input.Placeholder "expanded phone number field" ] ] ] ]
    //   // Subject field
    //  Field.div [ ]
    //       [ Label.label [ ]
    //           [ str "Subject" ]
    //         Control.div [ ]
    //           [ Select.select [ ]
    //               [ select [ DefaultValue "2" ]
    //                   [ option [ Value "1" ] [ str "Value n°1" ]
    //                     option [ Value "2"] [ str "Value n°2" ]
    //                     option [ Value "3"] [ str "Value n°3" ] ] ] ] ]

    //  // Terms and conditions area
    //  Field.div [ ]
    //       [ Control.div [ ]
    //           [ Checkbox.checkbox [ ]
    //               [ Checkbox.input [ ]
    //                 str "I agree with the terms and conditions" ] ] ]
    //  // Validation fields
    //  Field.div [ ]
    //       [ Control.div [ ]
    //           [ Radio.radio [ ]
    //               [ Radio.input [ Radio.Input.Name "answer" ]
    //                 str "Yes" ]
    //             Radio.radio [ ]
    //               [ Radio.input [ Radio.Input.Name "answer" ]
    //                 str "No" ] ] ]
    //  // Attachment
    //  Field.div [ ]
    //       [ File.file [ File.HasName ]
    //           [ File.label [ ]
    //               [ File.input [ ]
    //                 File.cta [ ]
    //                   [ File.icon [ ]
    //                       [ Icon.icon [ ]
    //                           [  ] ]
    //                     File.label [ ]
    //                       [ str "Choose a file..." ] ]
    //                 File.name [ ]
    //                   [ str "License agreement.pdf" ] ] ] ]

    //  // Control area (submit, cancel, etc.)
    //  Field.div [ Field.IsGrouped ]
    //       [ Control.div [ ]
    //           [ Button.button [ Button.Color IsPrimary ]
    //               [ str "Submit" ] ]
    //         Control.div [ ]
    //           [ Button.button [ Button.IsLink ]
    //               [ str "Cancel" ] ] ] ]
  )