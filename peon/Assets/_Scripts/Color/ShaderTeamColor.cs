namespace _Scripts.Color
{
    public static class ShaderTeamColor
    {
        public static UnityEngine.Color Red { get { return new UnityEngine.Color(1, 0, 0, 0.47f); } }
        public static UnityEngine.Color Blue { get { return new UnityEngine.Color(0, 0, 1, 0.47f); } }
        public static UnityEngine.Color Cyan { get { return new UnityEngine.Color(0, 1, 1, 0.47f); } }
        public static UnityEngine.Color Purple { get { return new UnityEngine.Color(0.549f, 0, 1, 0.47f); } }
        public static UnityEngine.Color Yellow { get { return new UnityEngine.Color(1, 1, 0, 0.47f); } }
        public static UnityEngine.Color Orange { get { return new UnityEngine.Color(1, 0.509f, 0, 0.686f); } }
        public static UnityEngine.Color Green { get { return new UnityEngine.Color(0, 1, 0, 0.47f); } }
        public static UnityEngine.Color Pink { get { return new UnityEngine.Color(1, 0, 0.568f, 0.47f); } }
        public static UnityEngine.Color Gray { get { return new UnityEngine.Color(0.235f, 0.235f, 0.235f, 0.47f); } }
        public static UnityEngine.Color LightBlue { get { return new UnityEngine.Color(0, 0.509f, 1, 0.47f); } }
        public static UnityEngine.Color DarkGreen { get { return new UnityEngine.Color(0, 0.47f, 0, 0.47f); } }
        public static UnityEngine.Color Brown { get { return new UnityEngine.Color(0.588f, 0.313f, 0, 0.686f); } }

        public static UnityEngine.Color ConvertColorNum(int num)
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
                _ => new UnityEngine.Color(0, 0, 0, 0),
            };
        }
    }
}
