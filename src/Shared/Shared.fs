namespace Shared

type Counter = { Value : int }

module CalendarUtil =
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
