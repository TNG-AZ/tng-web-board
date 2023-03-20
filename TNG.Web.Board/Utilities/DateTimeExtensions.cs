using Elfie.Serialization;

namespace TNG.Web.Board.Utilities
{
    public static class DateTimeExtensions
    {
        public static DateTime? ToAZTime(this DateTime? source)
            => source?.ToAZTime();

        public static DateTime ToAZTime(this DateTime source)
        {
            if (source == default) return default;
            var serverOffset = TimeZoneInfo.Local.BaseUtcOffset;
            var azTz = TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time");
            var offset = azTz.BaseUtcOffset;
            var newDt = source - serverOffset + offset;
            return newDt;
        }

    }
}
