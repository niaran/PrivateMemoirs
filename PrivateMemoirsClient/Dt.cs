using System;

namespace PrivateMemoirsClient
{
    public static class Dt
    {
        public static DateTime DateEnd
        {
            get { return DateTime.Today.AddDays(60); }
        }

        public static DateTime DateStart
        {
            get { return DateTime.Today; }
        }

        public static string DateTodayLong
        {
            get { return DateTime.Today.ToLongDateString(); }
        }
    }
}