using Models;

namespace BlazorApp.Services
{
    public static class Utils
    {
        public static string GetFormatedTimeAndDate(DateTime date)
        {
            return date.ToString(@"dd\/MM\/yyyy HH:mm tt");
        }

        public static string GetLastVisit(Visitor visitor)
        {

            if (visitor.LastVisit == null)
                return "No previous visits";

            var lastVisitDate = visitor.LastVisit.Value.ToLocalTime();
            return GetFormatedTimeAndDate(lastVisitDate);
        }

        public static string CheckOutText(AttendanceRecord record)
        {
            if (record.CheckOutTime.HasValue)
            {
                return GetFormatedTimeAndDate(record.CheckOutTime.Value);
            }
            return record.CheckInTime.Date == DateTime.UtcNow.Date ? "On Site" : "Never checked out";
        }
    }
}
