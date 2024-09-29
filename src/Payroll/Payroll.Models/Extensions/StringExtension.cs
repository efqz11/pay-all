using Microsoft.AspNetCore.Http;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Payroll
{
    public static class StringExtension
    {
        public static string AddOrdinal(this int num, bool overrideSpecificToPayroll = false)
        {
            if (num <= 0) return num.ToString();
            if (overrideSpecificToPayroll && num == 1)
                return "Start of month";
            if (overrideSpecificToPayroll && num == 31)
                return "End of month";

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }


        public static ExpandoObject ToExpando(this object obj)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj.GetType()))
            {
                expando.Add(property.Name, property.GetValue(obj));
            }

            return (ExpandoObject)expando;
        }

        public static List<ExpandoObject> ToExpandoObject(this List<dynamic> obj)
        {
            // Null-check
            var dicr = new List<ExpandoObject>();
            foreach (var item in obj)
            {
                dicr.Add(item.ToExpando());
            }
            return dicr;
        }


        /// <summary>
        /// Determines whether the specified HTTP request is an AJAX request.
        /// </summary>
        /// 
        /// <returns>
        /// true if the specified HTTP request is an AJAX request; otherwise, false.
        /// </returns>
        /// <param name="request">The HTTP request.</param><exception cref="T:System.ArgumentNullException">The <paramref name="request"/> parameter is null (Nothing in Visual Basic).</exception>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
            return false;
        }

        public static string GetCssStyleFromPercent(this int percent) =>
            "progress_" + (percent <= 5 ? 5 : percent <= 25 ? 25 : percent <= 50 ? 50 : percent <= 75 ? 75 : 100);

        public static string ToLocalFormat(this DateTime? date, bool includeTime = false)
        {
            if (date.HasValue == false) return "";
            return ToLocalFormat(date.Value, includeTime);
        }


        public static string ToLocalFormat(this DateTime date, bool includeTime = false, bool includeDayOfWeek = false)
        {
            //return date?.ToString("ddd, MMM dd, yyyy HH:mm");
            var str = date == DateTime.Now.Date ? "Today" : date.Date == DateTime.Now.AddDays(1).Date ? "Tomorrow" : date.Date == DateTime.Now.AddDays(-1).Date ? "Yesterday" : date.ToString($"{(includeDayOfWeek ? "ddd, " : "")}MMM dd, yyyy");

            if (includeTime)
                str += date.ToString(" HH:mm:ss");

            return str;
        }

        public static string ToSystemFormat(this DateTime? date, ClaimsPrincipal princial, bool includeTime = false)
        {
            if (date.HasValue)
                return ToSystemFormat(date.Value, princial, includeTime);
            return "";
        }

        public static DateTime ConvertToTimeZone(this DateTime date, ClaimsPrincipal princial)
        {
            //if (date.Kind != DateTimeKind.Utc)
            //    return date;

            var timeZone = princial.FindFirstValue(CustomClaimTypes.accessgrant_companyTimeZone);
            TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(date, timeInfo);
        }

        private static DateTime? ConvertToTimeZone(ClaimsPrincipal princial, DateTime? date)
        {
            if (!date.HasValue)
                return (DateTime?)null;

            return ConvertToTimeZone(princial, date);
        }

        public static string ToSystemFormat(this DateTime date, ClaimsPrincipal princial, bool includeTime = false, bool onlyTime = false, string formatOverride = null, bool convertToLocal = true)
        {
            if(convertToLocal)
            date = ConvertToTimeZone(date, princial);

            if (onlyTime )
                return date.ToString(formatOverride ?? princial.FindFirstValue(CustomClaimTypes.formatter_time)); 

            //return date?.ToString("ddd, MMM dd, yyyy HH:mm");
            var str = date.ToString(princial.FindFirstValue(CustomClaimTypes.formatter_date));

            if (includeTime)
                str += " " + date.ToString(princial.FindFirstValue(CustomClaimTypes.formatter_time));

            return str;
        }


        public static string ToSystemFormat(this decimal num, ClaimsPrincipal princial)
        {
            return string.Format(princial.FindFirstValue(CustomClaimTypes.formatter_number), num);
        }

        public static string ToSystemFormat(this double num, ClaimsPrincipal princial)
        {
            return string.Format(princial.FindFirstValue(CustomClaimTypes.formatter_number), num);
        }

        public static string StringFormat(this string format, IDictionary<string, string> values)
        {
            if (string.IsNullOrWhiteSpace(format)) return "";

            while(format.IndexOf("{") >= 0)
            {
                var _start = format.IndexOf("{");
                var palceHolder = format.Substring(_start, format.IndexOf("}") - _start).Replace("{", "");
                //if (values.ContainsKey(palceHolder))
                {
                    format = format.Replace("{" + palceHolder + "}", values.ContainsKey(palceHolder) ? values[palceHolder] : "Undefined");
                    return format.StringFormat(values);
                }
                    
            }
            //foreach (var p in values)
            //    format = format.Replace("{" + p.Key + "}", p.Value);
            return format;
        }

        public static string GetDuration(this DateTime? start, DateTime? End, ClaimsPrincipal princial, bool countDays = true)
        {
            if (start.HasValue == false || End.HasValue == false) return "";
            return GetDuration(start.Value, End.Value, princial, countDays);
        }

        public static string GetDuration(this DateTime start, DateTime? End, ClaimsPrincipal princial, bool countDays = true)
        {
            if (End.HasValue == false) return "";
            return GetDuration(start, End.Value, princial, countDays);
        }

        public static string GetDuration(this DateTime Start, DateTime End, ClaimsPrincipal princial, bool countDays = true)
        {
            Start = Start.ConvertToTimeZone(princial);
            End = End.ConvertToTimeZone(princial);
            try
            {
                var days = (End.Date - Start.Date).Days;
                if (days <= 0)
                    return Start.ToString("ddd, MMM dd, yyyy");

                var str = Start.ToString("MMM dd");
                if (Start.Year != End.Year)
                    str += Start.ToString(", yyyy");

                str += " — ";
                if (Start.Month == End.Month && Start.Year == End.Year)
                    str += End.ToString("dd, yyyy");
                else if (Start.Month != End.Month && Start.Year == End.Year)
                    str += End.ToString("MMM dd, yyyy");
                else
                    str += End.ToString("MMM dd, yyyy");

                str += (!countDays ? "" : " (" + days + " days)");
                return str;
            }
            catch (Exception)
            {
                return "";
            }
        }


        public static string GetHourMinString(this double hour, bool isMinutes = false)
        {
            if (isMinutes)
                hour = TimeSpan.FromMinutes(hour).TotalHours;

            var timeWorkedThisWeek = TimeSpan.FromHours(hour);
            return $"{timeWorkedThisWeek.TotalHours.ToString("N0")}h" + (timeWorkedThisWeek.Minutes <= 0 ? "" : $" {timeWorkedThisWeek.Minutes.ToString("N0")}m");
        }


        /// <summary>
        /// Get Time difference betwee 2 dates, return string like 2 seconds
        /// Supply end date if required, else leave blank to set it as current date/time
        /// </summary>
        /// <param name="dateStart"></param>
        /// <param name="dateEnd"></param>
        /// <returns></returns>
        public static string GetTimeDifference(this DateTime dateStart, DateTime? _dateEnd = null, ClaimsPrincipal princial = null)
        {
            if(princial != null)
                dateStart = dateStart.ConvertToTimeZone(princial);
            var dateEnd = _dateEnd ?? DateTime.Now;
            var duration = dateStart <= dateEnd ? dateEnd - dateStart : dateStart - dateEnd;
            var totalMonths = dateStart <= dateEnd ? (int)dateStart.TotalMonths(dateEnd) : (int)dateEnd.TotalMonths(dateStart);
            var totalYEars = totalMonths / 12f;
            var result = "";
            if (duration.TotalSeconds < 50)
                return "just now";
           // if (dateStart <= dateEnd) // start date is greater than end date
            {
                result += "";
                result += duration.TotalSeconds < 60
                    ? OptimiseSingularity((int)duration.TotalSeconds, "second")
                    : duration.TotalMinutes < 60
                        ? OptimiseSingularity((int)duration.TotalMinutes, "minute")
                        : duration.TotalHours < 24
                            ? OptimiseSingularity((int)duration.TotalHours, "hour")
                            : duration.TotalDays <= 7
                                ? OptimiseSingularity((int)duration.TotalDays, "day")

                                // weeks
                                : duration.TotalDays <= 14
                                    ? "a week"
                                    : duration.TotalDays <= 21
                                        ? "2 weeks"
                                        : duration.TotalDays <= 28
                                            ? "3 weeks"

                                            // months
                                            : totalMonths == 1
                                                ? "a months"
                                                : totalMonths == 2
                                                    ? "2 months"
                                                    : totalMonths == 3
                                                        ? "3 months"

                                            // years
                                            : totalYEars <= 1
                                                ? "a year"
                                                : totalYEars <= 2
                                                    ? "2 years"
                                                    : totalYEars <= 3
                                                        ? "3 years"
                                                            : totalYEars <= 4
                                                                ? "4 years"
                                                                : totalYEars <= 5
                                                                    ? "5 years"
                                                                    : "5+ years";
            }
            //else
            //{
            //    var compare = TimeSpan.Compare(dateStart.TimeOfDay, dateEnd.TimeOfDay);
            //    // -1  if  t1 is shorter than t2.
            //    //  0   if  t1 is equal to t2.
            //    //  1   if  t1 is longer than t2.

            //    if (compare == -1)
            //    {
            //        TimeSpan span = dateEnd.Subtract(dateStart);
            //        result = span.TotalHours > 0 ? (int)span.TotalHours + " hours" + (int)span.TotalMinutes + " minutes" : (int)span.TotalMinutes + " minutes";
            //    }
            //    else if (compare == 1) result = "work has started just now";
            //    else
            //    {
            //        TimeSpan span = dateStart.Subtract(dateEnd);
            //        result = span.TotalHours > 0 ? (int)span.TotalHours + " hours" + (int)span.TotalMinutes + " minutes" : (int)span.TotalMinutes + " minutes";
            //    }
            //}
            return result;
        }


        public static decimal TotalMonths(this DateTime startDate, DateTime endDate)
        {
            return (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;
        }

        private static string OptimiseSingularity(double totals, string template)
        {
            int item = (int)totals;
            return item <= 1
                ? $"{item} {template}"
                : $"{item} {template}s";
        }


        public static string LimitTo(this string phrase, int len = 35)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return "";

            if (phrase.Count() >= len)
                return phrase.Substring(0, len) + "...";
            return phrase;
        }

        public static string Escape(this string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return "";

            return phrase.Replace("\r", "<br>").Replace("\t", "<br>");
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            try
            {
                return enumValue.GetType()
                                .GetMember(enumValue.ToString())
                                .First()
                                .GetCustomAttribute<DisplayAttribute>()?.Name ?? enumValue.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static DisplayAttribute GetDisplayAttribute(this Enum enumValue) => GetAttribute<DisplayAttribute>(enumValue);

        /// <summary>
        ///     A generic extension method that aids in reflecting 
        ///     and retrieving any attribute that is applied to an `Enum`.
        /// </summary>
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
                where TAttribute : Attribute
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<TAttribute>();
        }
    }
}
