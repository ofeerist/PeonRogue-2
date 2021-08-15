namespace _Scripts.Color
{
    public static class TextTeamColor
    {
        public static string Red => "#FF0303";
        public static string Blue => "#0042FF";
        public static string Cyan => "#1CE6B9";
        public static string Purple => "#540081";
        public static string Yellow => "#FFFC01";
        public static string Orange => "#fEBA0E";
        public static string Green => "#20C000";
        public static string Pink => "#E55BB0";
        public static string Gray => "#959697";
        public static string LightBlue => "#7EBFF1";
        public static string DarkGreen => "#106246";
        public static string Brown => "#4E2A04";

        private static readonly string[] _colors = { Red, Blue, Cyan, Purple, Yellow, Orange, Green, Pink, Gray, LightBlue, DarkGreen, Brown };
        public static string ConvertColorNum(int num) => _colors[num];
    }
}
