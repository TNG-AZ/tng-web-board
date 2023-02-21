using Elfie.Serialization;

namespace TNG.Web.Board.Utilities
{
    public static class DateTimeExtensions
    {
        public static DateTime? ToAZTime(this DateTime? source)
        {
            if (source == null) return null;
            var serverOffset = TimeZoneInfo.Local.BaseUtcOffset;
            var azTz = TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time");
            var offset = azTz.BaseUtcOffset;
            var newDt = source - serverOffset + offset;
            return newDt;
        }
    }
}
