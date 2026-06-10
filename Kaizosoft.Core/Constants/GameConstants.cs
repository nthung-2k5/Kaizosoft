namespace Kaizosoft.Core.Constants;

/// <summary>
/// Contains constant values used throughout the Kaizosoft game asset management system.
/// </summary>
public static class GameConstants
{
    public const string DATA_LANGUAGE_RESOURCE_PATH = "data/language";
    public const string DATA_LANGUAGE_TEMPLATE_EN_RESOURCE_PATH = "data/language_pack_template_en";
    public const string CLASS_DATA_PACKAGE_PATH = "classdata.tpk";
    public const string GLOBAL_GAME_MANAGERS_FILE_NAME = "globalgamemanagers";

    public static class LanguageFileKeys
    {
        public const string APPLICATION_ID = "@appli";
        public const string VERSION = "@version";
        public const string TITLE = "@title";
        public const string LANGUAGE = "@language";
        public const string SCALE = "@scale";
        public const string AUTHOR = "@author";
    }

    public static class DefaultValues
    {
        public const string SCALE = "auto";
        public const string AUTHOR = "Kairosoft";
    }
}
