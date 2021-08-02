namespace _Scripts.Color
{
    public static class TextTeamColor
    {
        public static string Red { get { return "#FF0303"; } }
        public static string Blue { get { return "#0042FF"; } }
        public static string Cyan { get { return "#1CE6B9"; } }
        public static string Purple { get { return "#540081"; } }
        public static string Yellow { get { return "#FFFC01"; } }
        public static string Orange { get { return "#fEBA0E"; } }
        public static string Green { get { return "#20C000"; } }
        public static string Pink { get { return "#E55BB0"; } }
        public static string Gray { get { return "#959697"; } }
        public static string LightBlue { get { return "#7EBFF1"; } }
        public static string DarkGreen { get { return "#106246"; } }
        public static string Brown { get { return "#4E2A04"; } }

        public static string ConvertColorNum(int num)
        {
            return num switch
            {
                0 => Red,
                1 => Blue,
                2 => Cyan,
                3 => Purple,
                4 => Yellow,
                5 => Orange,
                6 => Green,
                7 => Pink,
                8 => Gray,
                9 => LightBlue,
                10 => DarkGreen,
                11 => Brown,
                _ => "white",
            };
        }
    }
}
