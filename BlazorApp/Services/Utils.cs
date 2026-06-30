using Models;
using System.Runtime.InteropServices;

namespace BlazorApp.Services
{
    public static class Utils
    {
        // Ireland uses "GMT Standard Time" on Windows and "Europe/Dublin" on Linux/Mac.
        // Since the server may be hosted anywhere in Europe, we must convert UTC times
        // explicitly to Irish time rather than relying on the server's local timezone.
        private static readonly TimeZoneInfo IrishTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "GMT Standard Time" : "Europe/Dublin");

        private static DateTime ToIrishTime(DateTime utcDate)
        {
            var utc = utcDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(utcDate, DateTimeKind.Utc)
                : utcDate.ToUniversalTime();
            return TimeZoneInfo.ConvertTimeFromUtc(utc, IrishTimeZone);
        }

        public static string GetFormatedTimeAndDate(DateTime date)
        {
            return ToIrishTime(date).ToString(@"dd\/MM\/yyyy HH:mm tt");
        }

        public static string GetLastVisit(Visitor visitor)
        {
            if (visitor.LastVisit == null)
                return "No previous visits";

            return GetFormatedTimeAndDate(visitor.LastVisit.Value);
        }

        public static string CheckOutText(AttendanceRecord record)
        {
            if (record.CheckOutTime.HasValue)
            {
                return GetFormatedTimeAndDate(record.CheckOutTime.Value);
            }
            return ToIrishTime(record.CheckInTime).Date == ToIrishTime(DateTime.UtcNow).Date
                ? "On Site"
                : "Never checked out";
        }
    }
}
