namespace FirstBattle
{
    public partial class GameEntry
    {
        public static BuiltinDataComponent BuiltinData
        {
            get;
            private set;
        }

        public static GameSaveComponent GameSave
        {
            get;
            private set;
        }

        private static void InitCustomComponents()
        {
            BuiltinData = UnityGameFramework.Runtime.GameEntry.GetComponent<BuiltinDataComponent>();
            GameSave = UnityGameFramework.Runtime.GameEntry.GetComponent<GameSaveComponent>();
        }
    }
}