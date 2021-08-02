namespace _Scripts
{
    public static class PrefsConstants
    {
        public static string Nickname = "Nickname";
        public static string Color = "Color";
        public static string QualityLevel = "QualityLevel";

        public static string MasterVolume = "MasterVolume";
        public static string SFXVolume = "SFXVolume";
        public static string MusicVolume = "MusicVolume";
        public static string AmbientVolume = "AmbientVolume";
        public static string MenuVolume = "MenuVolume";
        public static string UnitVolume = "UnitVolume";

        public static string[] Volumes { get { return new string[6] { MasterVolume, SFXVolume, MusicVolume, AmbientVolume, MenuVolume, UnitVolume }; } }
    }
}
