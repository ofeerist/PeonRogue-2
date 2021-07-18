using UnityEngine;

namespace Game.Colorizing
{
    public static class ShaderTeamColor
    {
        public static Color Red { get { return new Color(1, 0, 0, 0.47f); } }
        public static Color Blue { get { return new Color(0, 0, 1, 0.47f); } }
        public static Color Cyan { get { return new Color(0, 1, 1, 0.47f); } }
        public static Color Purple { get { return new Color(0.549f, 0, 1, 0.47f); } }
        public static Color Yellow { get { return new Color(1, 1, 0, 0.47f); } }
        public static Color Orange { get { return new Color(1, 0.509f, 0, 0.686f); } }
        public static Color Green { get { return new Color(0, 1, 0, 0.47f); } }
        public static Color Pink { get { return new Color(1, 0, 0.568f, 0.47f); } }
        public static Color Gray { get { return new Color(0.235f, 0.235f, 0.235f, 0.47f); } }
        public static Color LightBlue { get { return new Color(0, 0.509f, 1, 0.47f); } }
        public static Color DarkGreen { get { return new Color(0, 0.47f, 0, 0.47f); } }
        public static Color Brown { get { return new Color(0.588f, 0.313f, 0, 0.686f); } }

        public static Color ConvertColorNum(int num)
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
                _ => new Color(0, 0, 0, 0),
            };
        }
    }
}
