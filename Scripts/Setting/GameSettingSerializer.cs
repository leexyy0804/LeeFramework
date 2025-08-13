using GameFramework;

namespace FirstBattle
{
    public class GameSettingSerializer : GameFrameworkSerializer<GameSetting>
    {
        private static readonly byte[] Header = new byte[] { (byte)'G', (byte)'F', (byte)'S' };

        public GameSettingSerializer()
        {

        }

        protected override byte[] GetHeader()
        {
            return Header;
        }
    }
}