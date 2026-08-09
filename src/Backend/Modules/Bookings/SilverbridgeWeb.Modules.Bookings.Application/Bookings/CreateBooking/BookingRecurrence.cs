using SilverbridgeWeb.Common.Domain;
using SilverbridgeWeb.Modules.Bookings.Domain.Bookings;

namespace SilverbridgeWeb.Modules.Bookings.Application.Bookings.CreateBooking;

internal static class BookingRecurrence
{
    private static readonly TimeZoneInfo BookingTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Dublin");

    public static Result<IReadOnlyCollection<BookingOccurrence>> CreateSchedule(
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        bool isRecurring,
        IReadOnlyCollection<DayOfWeek> recurrenceDays,
        DateTime? recurrenceEndDate)
    {
        if (!isRecurring)
        {
            return new[] { new BookingOccurrence(startsAtUtc, endsAtUtc) };
        }

        DateTime localStart = ToBookingTime(startsAtUtc);
        DateTime localEnd = ToBookingTime(endsAtUtc);
        var selectedDays = recurrenceDays.ToHashSet();

        if (recurrenceEndDate is null ||
            recurrenceEndDate.Value.Date < localStart.Date ||
            recurrenceEndDate.Value.Date > localStart.Date.AddDays(365) ||
            selectedDays.Count == 0 ||
            selectedDays.Any(day => !Enum.IsDefined(day)))
        {
            return Result.Failure<IReadOnlyCollection<BookingOccurrence>>(BookingErrors.InvalidRecurrence);
        }

        TimeSpan duration = localEnd - localStart;
        var occurrences = new List<BookingOccurrence>();

        try
        {
            for (DateTime date = localStart.Date;
                 date <= recurrenceEndDate.Value.Date;
                 date = date.AddDays(1))
            {
                if (!selectedDays.Contains(date.DayOfWeek))
                {
                    continue;
                }

                DateTime occurrenceStart = date.Add(localStart.TimeOfDay);
                DateTime occurrenceEnd = occurrenceStart.Add(duration);
                occurrences.Add(new BookingOccurrence(
                    ToUtc(occurrenceStart),
                    ToUtc(occurrenceEnd)));
            }
        }
        catch (ArgumentException)
        {
            return Result.Failure<IReadOnlyCollection<BookingOccurrence>>(BookingErrors.InvalidRecurrence);
        }

        return occurrences.Count == 0
            ? Result.Failure<IReadOnlyCollection<BookingOccurrence>>(BookingErrors.InvalidRecurrence)
            : occurrences;
    }

    private static DateTime ToBookingTime(DateTime utcDateTime) =>
        TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc),
            BookingTimeZone);

    private static DateTime ToUtc(DateTime bookingDateTime) =>
        TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(bookingDateTime, DateTimeKind.Unspecified),
            BookingTimeZone);
}

internal sealed record BookingOccurrence(DateTime StartsAtUtc, DateTime EndsAtUtc);
