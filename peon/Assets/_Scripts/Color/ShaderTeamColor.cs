namespace _Scripts.Color
{
    public static class ShaderTeamColor
    {
        public static UnityEngine.Color Red => new UnityEngine.Color(1, 0, 0, 1f);
        public static UnityEngine.Color Blue => new UnityEngine.Color(0, 0, 1, 1f);
        public static UnityEngine.Color Cyan => new UnityEngine.Color(0, 1, 1, 1f);
        public static UnityEngine.Color Purple => new UnityEngine.Color(0.6f, 0, 1, 1f);
        public static UnityEngine.Color Yellow => new UnityEngine.Color(1, 1, 0, 1f);
        public static UnityEngine.Color Orange => new UnityEngine.Color(1, 0.509f, 0, 1f);
        public static UnityEngine.Color Green => new UnityEngine.Color(0, 1, 0, 1f);
        public static UnityEngine.Color Pink => new UnityEngine.Color(1, 0, 0.568f, 1f);
        public static UnityEngine.Color Gray => new UnityEngine.Color(0.235f, 0.235f, 0.235f, 1f);
        public static UnityEngine.Color LightBlue => new UnityEngine.Color(0, 0.509f, 1, 1f);
        public static UnityEngine.Color DarkGreen => new UnityEngine.Color(0, 0.47f, 0, 1f);
        public static UnityEngine.Color Brown => new UnityEngine.Color(0.588f, 0.313f, 0, 1f);

        private static readonly UnityEngine.Color[] _colors = { Red, Blue, Cyan, Purple, Yellow, Orange, Green, Pink, Gray, LightBlue, DarkGreen, Brown };
        public static UnityEngine.Color ConvertColorNum(int num) => _colors[num];
    }
}
