using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using CalendarGenerator.Lesson;
using CalendarGenerator.PdfParse;

namespace CalendarGenerator.Calendar
{
    public class Calendar
    {
        public static string GenerateCalendar(string rawInput)
        {
            var daysList = PdfParser.GetDaysList(rawInput);
            IEnumerable<CalendarEvent> events =
                daysList.SelectMany(day => day.GetLessonTexts()).Select(text => text.ToCalendarEvent());

            StringBuilder calendar = new StringBuilder();
            calendar.AppendLine("BEGIN:VCALENDAR");
            calendar.AppendLine("VERSION:2.0");
            calendar.AppendLine("PRODID:Schedule_generated_with_itext7");
            calendar.AppendLine("CALSCALE:GREGORIAN");

            events.ToList().ForEach(ev =>
            {
                calendar.AppendLine("BEGIN:VEVENT");
                calendar.AppendLine("DTSTAMP:" + FormatDateTime(ev.TimeStamp));
                calendar.AppendLine("DTSTART:" + FormatDateTime(ev.Start));
                calendar.AppendLine("DTEND:" + FormatDateTime(ev.End));
                calendar.AppendLine("SUMMARY:" + ev.Summary);
                calendar.AppendLine("DESCRIPTION:" + ev.Description);
                calendar.AppendLine("LOCATION:" + ev.Location);
                calendar.AppendLine("UID:" + ev.Uid);
                calendar.AppendLine("END:VEVENT");
            });

            calendar.AppendLine("END:VCALENDAR");
            return calendar.ToString();
        }

        private static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.Year.ToString() + AppendZeroIfSingleDigit(dateTime.Month) +
                   AppendZeroIfSingleDigit(dateTime.Day) + "T" +
                   AppendZeroIfSingleDigit(dateTime.Hour) +
                   AppendZeroIfSingleDigit(dateTime.Minute) +
                   "00";
        }

        private static string AppendZeroIfSingleDigit(int number)
        {
            if (number >= 10) return number.ToString();
            return "0" + number;
        }
    }
}