using System;

namespace DeliveryAdmin.Helpers
{
    /// <summary>
    /// ✅ الأدمن دشبورد شغال Server-side (Razor)، يعني ".ToLocalTime()" مش مضمونة
    /// لأنها بتعتمد على توقيت السيرفر نفسه (اللي هو المشكلة الأصلية) مش على
    /// توقيت اللي فاتح الصفحة في مصر. عشان كده هنا لازم نحوّل لتوقيت مصر
    /// صراحة (Explicit) مش عن طريق ToLocalTime.
    /// </summary>
    public static class EgyptTimeZoneHelper
    {
        private static readonly TimeZoneInfo EgyptTimeZone = ResolveEgyptTimeZone();

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EgyptTimeZone);

        public static DateTime ToEgyptTime(DateTime utcDateTime)
        {
            var utc = utcDateTime.Kind == DateTimeKind.Utc
                ? utcDateTime
                : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utc, EgyptTimeZone);
        }

        private static TimeZoneInfo ResolveEgyptTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"); // Windows
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); // Linux/Docker
                }
                catch (TimeZoneNotFoundException)
                {
                    return TimeZoneInfo.CreateCustomTimeZone("Egypt_Fallback", TimeSpan.FromHours(2), "Egypt (fallback)", "Egypt (fallback)");
                }
            }
        }
    }
}
