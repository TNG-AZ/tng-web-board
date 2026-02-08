namespace TNG.Web.Board.Utilities
{
    public static class DateTimeExtensions
    {
        public static DateTime? ToAZTime(this DateTime? source)
            => source?.ToAZTime();

        public static DateTime ToAZTime(this DateTime source)
        {
            if (source == default) return default;
            var offset = source.Kind == DateTimeKind.Local
                ? TimeZoneInfo.Local.BaseUtcOffset
                : TimeZoneInfo.Utc.BaseUtcOffset;
            var azTz = TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time");
            var azOffset = azTz.BaseUtcOffset;
            var newDt = source - offset + azOffset;
            return newDt;
        }

    }
}
