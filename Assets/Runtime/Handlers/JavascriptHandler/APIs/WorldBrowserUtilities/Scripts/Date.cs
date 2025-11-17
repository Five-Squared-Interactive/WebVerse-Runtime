// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;

namespace FiveSQD.WebVerse.Handlers.Javascript.APIs.Utilities
{
    /// <summary>
    /// Class for a date and time.
    /// </summary>
    public class Date
    {
        /// <summary>
        /// Internal date and time object.
        /// </summary>
        private DateTime internalDateTime;

        /// <summary>
        /// Get a Date for the current millisecond.
        /// </summary>
        public static Date now
        {
            get
            {
                DateTime current = DateTime.Now;
                return new Date(current.Year, current.Month, current.Day,
                    current.Hour, current.Minute, current.Second, current.Millisecond);
            }
        }

        /// <summary>
        /// Year.
        /// </summary>
        public int year
        {
            get
            {
                return internalDateTime.Year;
            }
        }

        /// <summary>
        /// Month.
        /// </summary>
        public int month
        {
            get
            {
                return internalDateTime.Month;
            }
        }

        /// <summary>
        /// Day.
        /// </summary>
        public int day
        {
            get
            {
                return internalDateTime.Day;
            }
        }

        /// <summary>
        /// Day of week.
        /// </summary>
        public int dayOfWeek
        {
            get
            {
                switch (internalDateTime.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        return 0;

                    case DayOfWeek.Monday:
                        return 1;

                    case DayOfWeek.Tuesday:
                        return 2;

                    case DayOfWeek.Wednesday:
                        return 3;

                    case DayOfWeek.Thursday:
                        return 4;

                    case DayOfWeek.Friday:
                        return 5;

                    case DayOfWeek.Saturday:
                        return 6;

                    default:
                        return -1;
                }
            }
        }

        /// <summary>
        /// Day of year.
        /// </summary>
        public int dayOfYear
        {
            get
            {
                return internalDateTime.DayOfYear;
            }
        }

        /// <summary>
        /// Hour.
        /// </summary>
        public int hour
        {
            get
            {
                return internalDateTime.Hour;
            }
        }

        /// <summary>
        /// Minute.
        /// </summary>
        public int minute
        {
            get
            {
                return internalDateTime.Minute;
            }
        }

        /// <summary>
        /// Second.
        /// </summary>
        public int second
        {
            get
            {
                return internalDateTime.Second;
            }
        }

        /// <summary>
        /// Millisecond.
        /// </summary>
        public int millisecond
        {
            get
            {
                return internalDateTime.Millisecond;
            }
        }

        /// <summary>
        /// Constructor for a Date.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month.</param>
        /// <param name="day">Day.</param>
        public Date(int year, int month, int day)
        {
            internalDateTime = new DateTime(year, month, day);
        }

        /// <summary>
        /// Constructor for a Date.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month.</param>
        /// <param name="day">Day.</param>
        /// <param name="hours">Hours.</param>
        public Date(int year, int month, int day, int hours)
        {
            internalDateTime = new DateTime(year, month, day, hours, 0, 0);
        }

        /// <summary>
        /// Constructor for a Date.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month.</param>
        /// <param name="day">Day.</param>
        /// <param name="hours">Hours.</param>
        /// <param name="minutes">Minutes.</param>
        public Date(int year, int month, int day, int hours, int minutes)
        {
            internalDateTime = new DateTime(year, month, day, hours, minutes, 0);
        }

        /// <summary>
        /// Constructor for a Date.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month.</param>
        /// <param name="day">Day.</param>
        /// <param name="hours">Hours.</param>
        /// <param name="minutes">Minutes.</param>
        /// <param name="seconds">Seconds.</param>
        public Date(int year, int month, int day, int hours, int minutes, int seconds)
        {
            internalDateTime = new DateTime(year, month, day, hours, minutes, seconds);
        }

        /// <summary>
        /// Constructor for a Date.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month.</param>
        /// <param name="day">Day.</param>
        /// <param name="hours">Hours.</param>
        /// <param name="minutes">Minutes.</param>
        /// <param name="seconds">Seconds.</param>
        /// <param name="milliseconds">Milliseconds.</param>
        public Date(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds)
        {
            internalDateTime = new DateTime(year, month, day, hours, minutes, seconds, milliseconds);
        }

        /// <summary>
        /// Constructor for a Date.
        /// </summary>
        /// <param name="dateString">Date string.</param>
        public Date(string dateString)
        {
            internalDateTime = DateTime.Parse(dateString);
        }

        /// <summary>
        /// Get a string representation of the complete date and time.
        /// </summary>
        /// <returns>A string representation of the complete date and time.</returns>
        public override string ToString()
        {
            return internalDateTime.ToString();
        }

        /// <summary>
        /// Get a string representation of the date.
        /// </summary>
        /// <returns>A string representation of the date.</returns>
        public string ToDateString()
        {
            return internalDateTime.ToLongDateString();
        }

        /// <summary>
        /// Get a string representation of the time.
        /// </summary>
        /// <returns>A string representation of the time.</returns>
        public string ToTimeString()
        {
            return internalDateTime.ToLongTimeString();
        }

        /// <summary>
        /// Get a string representation of the complete UTC date and time.
        /// </summary>
        /// <returns>A string representation of the complete UTC date and time.</returns>
        public string ToUTCString()
        {
            return internalDateTime.ToUniversalTime().ToString(@"M/d/yyyy hh:mm:ss tt");
        }
    }
}