using System;
using System.IO;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace LeeFramework.Scripts.GameSave
{
    public sealed class GameSaveInfo
    {
        private const string GameSaveDataFileName = "GameSave{0}_{1}.dat";
        private const string GameSaveInfoFileName = "GameSave{0}_{1}.info";

        public int GameSaveId { get; set; }
        public int SerialId { get; set; }
        public string GameSaveName { get; set; }
        public float PlayTime { get; set; }
        public DateTime SaveTime { get; set; }
        public string GameVersion { get; set; }
        public string PlayerName { get; set; }
        public int PlayerLevel { get; set; }
        public string SceneName { get; set; }
        public Sprite ScreenShot { get; set; }
        public string SavePath { get; private set; }

        private GameSaveInfo()
        {
        }

        public static GameSaveInfo CreateWithGameSave(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    GameSaveInfo info = CreateNewGameSave();
                    info.SavePath = Path.GetDirectoryName(filePath);
                    info.Deserialize(reader);
                    return info;
                }
        }

        public static GameSaveInfo CreateNewGameSave()
        {
            GameSaveInfo info = new GameSaveInfo();
            return info;
        }

        public string GetSaveDataFilePath()
        {
            if (string.IsNullOrEmpty(SavePath))
            {
                return null;
            }

            string timestamp = SaveTime.ToString("yyyyMMdd_HHmmss");
            return Path.Combine(SavePath, string.Format(GameSaveDataFileName, SerialId, timestamp));
        }

        public void Save(string filePath)
        {
            SavePath = filePath;
            filePath = Path.Combine(filePath, string.Format(GameSaveInfoFileName, SerialId, SaveTime.ToString("yyyyMMdd_HHmmss")));
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    Serialize(writer);
                }
            Log.Info("Game save info saved to '{0}'.", filePath);
        }

        public static int GenerateNewSerialId()
        {
            // 使用当前时间戳的低32位作为基础
            long timestamp = DateTime.Now.Ticks;
            int baseId = (int)(timestamp & 0xFFFFFFFF);

            // 添加一个0-999的随机数，确保同一毫秒内的存档也有不同的ID
            int random = UnityEngine.Random.Range(0, 1000);

            // 组合时间戳和随机数
            return baseId + random;
        }

        public void Serialize(BinaryWriter writer)
        {
            try
            {
                writer.Write(GameSaveId);
                writer.Write(SerialId);
                writer.Write(GameSaveName ?? string.Empty);
                writer.Write(PlayTime);
                writer.Write(SaveTime.Ticks);
                writer.Write(GameVersion ?? string.Empty);
                writer.Write(PlayerName ?? string.Empty);
                writer.Write(PlayerLevel);
                writer.Write(SceneName ?? string.Empty);

                // 序列化截图
                if (ScreenShot != null)
                {
                    writer.Write(true);
                    byte[] screenshotBytes = ScreenShotToBytes(ScreenShot);
                    writer.Write(screenshotBytes.Length);
                    writer.Write(screenshotBytes);
                }
                else
                {
                    writer.Write(false);
                }
            }
            catch (Exception exception)
            {
                Log.Error("Serialize GameSaveInfo failed with exception '{0}'.", exception);
                throw;
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            try
            {
                GameSaveId = reader.ReadInt32();
                SerialId = reader.ReadInt32();
                GameSaveName = reader.ReadString();
                PlayTime = reader.ReadSingle();
                SaveTime = new DateTime(reader.ReadInt64());
                GameVersion = reader.ReadString();
                PlayerName = reader.ReadString();
                PlayerLevel = reader.ReadInt32();
                SceneName = reader.ReadString();

                // 反序列化截图
                bool hasScreenshot = reader.ReadBoolean();
                if (hasScreenshot)
                {
                    int screenshotLength = reader.ReadInt32();
                    byte[] screenshotBytes = reader.ReadBytes(screenshotLength);
                    ScreenShot = BytesToSprite(screenshotBytes);
                }
                else
                {
                    ScreenShot = null;
                }
            }
            catch (Exception exception)
            {
                Log.Error("Deserialize GameSaveInfo failed with exception '{0}'.", exception);
                throw;
            }
        }

        private byte[] ScreenShotToBytes(Sprite sprite)
        {
            try
            {
                Texture2D texture = sprite.texture;
                return texture.EncodeToPNG();
            }
            catch (Exception exception)
            {
                Log.Error("Convert screenshot to bytes failed with exception '{0}'.", exception);
                return null;
            }
        }

        private Sprite BytesToSprite(byte[] bytes)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
            }
            catch (Exception exception)
            {
                Log.Error("Convert bytes to sprite failed with exception '{0}'.", exception);
                return null;
            }
        }
    }
}