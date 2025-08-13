using GameFramework;

namespace FirstBattle
{
    public static class AssetUtility
    {
        public static string GetConfigAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/FirstBattle/GameMain/Configs/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetDataTableAsset(string assetName, bool fromBytes)
        {
            if (fromBytes)
            {
                return Utility.Text.Format("Assets/FirstBattle/GameMain/DataTables/{0}.bytes", assetName);
            }
            return Utility.Text.Format("Assets/FirstBattle/GameMain/DataTables/Txt/{0}.txt", assetName);
        }

        public static string GetDictionaryAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/FirstBattle/GameMain/Localization/{0}/Dictionaries/{1}.{2}", GameEntry.Localization.Language, assetName, fromBytes ? "bytes" : "xml");
        }

        public static string GetFontAsset(string assetName)
        {
            return Utility.Text.Format("Assets/FirstBattle/GameMain/Fonts/{0}.ttf", assetName);
        }

        public static string GetSceneAsset(string assetName)
        {
            return Utility.Text.Format("Assets/FirstBattle/GameMain/Scenes/{0}.unity", assetName);
        }

        public static string GetMusicAsset(string assetName)
        {
            return Utility.Text.Format("Assets/FirstBattle/GameMain/Music/{0}.mp3", assetName);
        }

        public static string GetSoundAsset(string assetName)
        {
            return Utility.Text.Format("Assets/FirstBattle/GameMain/Sounds/{0}.wav", assetName);
        }

        public static string GetEntityAsset(string assetName)
        {
            return Utility.Text.Format("Assets/FirstBattle/GameMain/Entities/{0}.prefab", assetName);
        }

        public static string GetUIFormAsset(string assetName)
        {
            return Utility.Text.Format("Assets/FirstBattle/GameMain/UI/UIForms/{0}.prefab", assetName);
        }

        public static string GetUIItemAsset(string assetName)
        {
            return Utility.Text.Format("Assets/FirstBattle/GameMain/UI/UIItems/{0}.prefab", assetName);
        }

        public static string GetUISoundAsset(string assetName)
        {
            return Utility.Text.Format("Assets/FirstBattle/GameMain/UI/UISounds/{0}.wav", assetName);
        }
    }
}