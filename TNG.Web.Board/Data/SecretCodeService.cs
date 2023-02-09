namespace TNG.Web.Board.Data
{
    public static class SecretCodeService
    {
        private static string? Code { get; set; }

        public static string? GetCode()
            => Code;

        public static void SetCode(string? code)
            => Code = code;
    }
}
