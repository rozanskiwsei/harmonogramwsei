using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("CalGeneratorTests"), InternalsVisibleTo("ConsoleCalGenerator")]

namespace CalendarGenerator.PdfParse
{
    internal class PdfParser
    {
        private const string DateWordPattern = "Data Zajęć:";
        internal const string DatePattern = "\\d\\d\\d\\d-\\d\\d-\\d\\d";
        private const string DayPattern = "\\b(poniedziałek|wtorek|środa|czwartek|piątek|sobota|niedziela)\\b";
        internal const string DateAndDayPatternLine = DateWordPattern + " +" + DatePattern + " +" + DayPattern;

        internal const string PossibleTitlesPattern =
            "(prof. zw. dr hab.|prof. WSEI dr hab.|prof. nadzw. dr|prof. dr hab. inż.|prof. dr hab.|mecenas|mgr inż.|mgr|dr inż.|inż.|dr hab. inż.|doc. dr|dr hab.|dr|MBA)";

        internal const string HoursPattern = "\\d?\\d:\\d\\d \\d?\\d:\\d\\d";
        internal const string PossibleLessonTypesPattern = " Cw | Lab | Konw | Wyk ";

        internal static List<Day> GetDaysList(string input)
        {
            var words = RawTextToWords(input);
            var stringDayItems = WordsToStringDayItems(words);
            return DayStringsToDayItems(stringDayItems);
        }

        internal static string[] RawTextToWords(string input)
        {
            var removedBreakLines = input.Replace("\n", " ");
            return removedBreakLines.Split(" ");
        }

        internal static List<string> WordsToStringDayItems(string[] words)
        {
            var stringBuilder = new StringBuilder();
            var dayItems = new List<string>();
            const string dayPattern = "Data";
            var afterTableHeader = false;

            foreach (var word in words)
            {
                var dayMatcher = Regex.Match(word, dayPattern);
                if (dayMatcher.Success)
                {
                    afterTableHeader = true;
                    if (stringBuilder.Length != 0) dayItems.Add(stringBuilder.ToString().Trim());
                    stringBuilder.Clear();
                    stringBuilder.Append(word + " ");
                }
                else if (afterTableHeader)
                {
                    stringBuilder.Append(word + " ");
                }
            }

            dayItems.Add(stringBuilder.ToString().Trim());
            return dayItems;
        }

        internal static List<Day> DayStringsToDayItems(List<string> dayStringItems)
        {
            var days = new List<Day>();
            dayStringItems.ForEach(dayStringItem =>
            {
                var date = ExtractDateFromDayStringItem(dayStringItem);
                var lessons = ExtractLessonStringsFromDayStringItem(dayStringItem);
                var day = new Day(date, lessons);
                days.Add(day);
            });
            return days;
        }

        internal static string ExtractDateFromDayStringItem(string dayStringItem)
        {
            var regex = new Regex(DateAndDayPatternLine);
            var match = regex.Match(dayStringItem.Replace("  ", " "));
            if (match.Success == false) throw new ParsingException(ParsingException.MatchingDateFailed);
            if (match.Index != 0) throw new ParsingException(ParsingException.IndexOfMatchedItemNotZero);
            return match.Value;
        }

        internal static List<string> ExtractLessonStringsFromDayStringItem(string dayStringItem)
        {
            var lessons = new List<string>();
            var regex = new Regex(HoursPattern);
            var matches = regex.Matches(dayStringItem);

            for (var i = 0; i < matches.Count - 1; i++)
            {
                var start = matches[i].Index;
                var length = matches[i].NextMatch().Index - start;
                lessons.Add(dayStringItem.Substring(start, length));
            }

            lessons.Add(dayStringItem.Substring(matches[^1].Index));
            return lessons;
        }
    }

    public class ParsingException : Exception
    {
        internal const string MatchingDateFailed = "Matching date not successful";
        internal const string IndexOfMatchedItemNotZero = "Index of matched item not equals to 0";
        internal const string MatchingLineToPatternFailed = "Matching line to pattern failed:";
        internal const string HeadersNotMatched = "Headers not matched:";

        public ParsingException(string message) : base(message)
        {
        }
    }
}