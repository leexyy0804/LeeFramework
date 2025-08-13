using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using GameFramework;
using UnityGameFramework.Runtime;

namespace FirstBattle
{
    public sealed partial class GameSave
    {
        private sealed class GameSaveGroup
        {
            private const string GameSaveInfoDirectoryName = "Save_{0}";
            private const string GameSaveInfoFileName = "GameSave{0}_{1}.info";
            private const string GameSaveDataFileName = "GameSave{0}_{1}.dat";
            private const int MaxSaveCount = 10; // 每个存档组最多保存10个存档点

            private string m_GameSaveName;

            public string GameSaveName
            {
                get => m_GameSaveName;
            }

            private int m_GameSaveId;

            public int GameSaveId
            {
                get => m_GameSaveId;
            }

            private List<GameSaveInfo> m_GameSaveInfos = new List<GameSaveInfo>();
            private GameSave.GameSaveData m_CurrentSaveData;

            // 添加游戏状态相关字段
            private float m_PlayTime;
            private string m_PlayerName;
            private int m_PlayerLevel;
            private string m_SceneName;

            public GameSaveInfo GetHeadInfo()
            {
                if (m_GameSaveInfos.Count > 0)
                {
                    return m_GameSaveInfos[0];
                }

                return null;
            }

            public GameSaveInfo[] GetAllSaveInfos()
            {
                return m_GameSaveInfos.ToArray();
            }

            // 添加更新游戏状态的方法
            public void UpdateGameState(float playTime, string playerName, int playerLevel, string sceneName)
            {
                m_PlayTime = playTime;
                m_PlayerName = playerName;
                m_PlayerLevel = playerLevel;
                m_SceneName = sceneName;
            }

            // 添加获取游戏状态的方法
            private (float playTime, string playerName, int playerLevel, string sceneName) GetGameState()
            {
                // 尝试从游戏数据中获取状态
                if (m_CurrentSaveData != null)
                {
                    float playTime = m_CurrentSaveData.GetData("PlayTime", m_PlayTime);
                    string playerName = m_CurrentSaveData.GetData("PlayerName", m_PlayerName);
                    int playerLevel = m_CurrentSaveData.GetData("PlayerLevel", m_PlayerLevel);
                    string sceneName = m_CurrentSaveData.GetData("SceneName", m_SceneName);
                    // 注意：Sprite不能直接序列化，需要特殊处理

                    return (playTime, playerName, playerLevel, sceneName);
                }

                return (m_PlayTime, m_PlayerName, m_PlayerLevel, m_SceneName);
            }

            private GameSaveGroup()
            {
                m_CurrentSaveData = GameSave.GameSaveData.CreateEmpty();
            }

            public static GameSaveGroup CreateGameSaveFromFile(string filePath)
            {
                if (string.IsNullOrEmpty(filePath) || !Directory.Exists(filePath))
                {
                    return null;
                }

                GameSaveGroup group = new GameSaveGroup();

                // 加载存档信息
                group.Load(filePath);

                group.m_GameSaveId = group.GetHeadInfo().GameSaveId;
                group.m_GameSaveName = group.GetHeadInfo().GameSaveName;

                return group;
            }

            public static GameSaveGroup CreateWithGameSave(int gameSaveId, string gameSaveName = "NewGameSave")
            {
                GameSaveGroup group = new GameSaveGroup();
                group.m_GameSaveId = gameSaveId;
                group.m_GameSaveName = gameSaveName;
                return group;
            }

            public bool Save(string filePath)
            {
                try
                {
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    // string infoPath = Path.Combine(
                    //     Path.Combine(filePath, string.Format(GameSaveInfoDirectoryName, m_GameSaveId)),
                    //     string.Format(GameSaveInfoFileName, m_GameSaveId, timestamp));
                    //string infoPath = Path.Combine(filePath, string.Format(GameSaveInfoFileName, m_GameSaveId, timestamp));

                    // 保存存档信息
                    string dataSavePath = SaveGameInfo(filePath);
                    if (string.IsNullOrEmpty(dataSavePath))
                    {
                        return false;
                    }

                    // 保存存档数据
                    if (!SaveGameData(dataSavePath))
                    {
                        return false;
                    }

                    return true;
                }
                catch (Exception exception)
                {
                    Log.Error("Save game failed with exception '{0}'.", exception);
                    return false;
                }
            }

            private bool SaveGameData(string filePath)
            {
                try
                {
                    if (m_CurrentSaveData == null)
                    {
                        return false;
                    }

                    // 序列化数据
                    byte[] data = SerializeGameDataToBytes();
                    if (data == null)
                    {
                        return false;
                    }

                    // 计算哈希值
                    string hash = GameSaveSerializer.CalculateHash(data);

                    // 加密数据
                    byte[] encryptedData = GameSaveSerializer.EncryptData(data);
                    if (encryptedData == null)
                    {
                        return false;
                    }

                    // 保存加密数据
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        using (BinaryWriter writer = new BinaryWriter(fs))
                        {
                            writer.Write(hash);
                            writer.Write(encryptedData.Length);
                            writer.Write(encryptedData);
                        }

                    return true;
                }
                catch (Exception exception)
                {
                    Log.Error("Save game data failed with exception '{0}'.", exception);
                    return false;
                }
            }

            private string SaveGameInfo(string filePath)
            {
                try
                {
                    // 创建并保存存档信息
                    GameSaveInfo saveInfo = GameSaveInfo.CreateNewGameSave();
                    saveInfo.GameSaveId = m_GameSaveId;
                    saveInfo.SerialId = GameSaveInfo.GenerateNewSerialId();
                    saveInfo.GameSaveName = m_GameSaveName;
                    saveInfo.SaveTime = DateTime.Now;
                    saveInfo.GameVersion = Application.version;
                    saveInfo.PlayTime = m_PlayTime;
                    saveInfo.PlayerName = m_PlayerName;
                    saveInfo.PlayerLevel = m_PlayerLevel;
                    saveInfo.SceneName = m_SceneName;
                    saveInfo.ScreenShot = CaptureScreenShot();

                    saveInfo.Save(filePath);
                    Log.Debug("game save info saved. playTime = {0}, serialId = {1}", saveInfo.PlayTime, saveInfo.SerialId);

                    m_GameSaveInfos.Add(saveInfo);
                    return saveInfo.GetSaveDataFilePath();
                }
                catch (Exception exception)
                {
                    Log.Error("Save game info failed with exception '{0}'.", exception);
                    return "";
                }
            }

            public bool Load(string gameSavePath)
            {
                try
                {
                    if (!Directory.Exists(gameSavePath))
                    {
                        return false;
                    }

                    // 查找最新的存档文件
                    string[] infoFiles = Directory.GetFiles(gameSavePath,
                        string.Format(GameSaveInfoFileName, "*", "*"));
                    if (infoFiles.Length == 0)
                    {
                        return false;
                    }

                    foreach (var file in infoFiles)
                    {
                        LoadGameInfo(file);
                    }
                    m_GameSaveInfos.Sort((x, y) =>
                    {
                        return y.SaveTime.CompareTo(x.SaveTime);
                    });

                    foreach (var saveInfo in m_GameSaveInfos)
                    {
                        Log.Info("save info loaded. serialId = {0}, playTime = {1}, saveTime = {2}",
                            saveInfo.SerialId, saveInfo.PlayTime, saveInfo.SaveTime);
                    }

                    return true;
                }
                catch (Exception exception)
                {
                    Log.Error("Load game failed with exception '{0}'.", exception);
                    return false;
                }
            }

            private bool LoadGameInfo(string filePath)
            {
                try
                {
                    if (!File.Exists(filePath))
                    {
                        return false;
                    }

                    GameSaveInfo saveInfo = GameSaveInfo.CreateWithGameSave(filePath);
                    if (saveInfo != null)
                    {
                        m_GameSaveInfos.Add(saveInfo);
                        return true;
                    }

                    return false;
                }
                catch (Exception exception)
                {
                    Log.Error("Load game info failed with exception '{0}'.", exception);
                    return false;
                }
            }

            public GameSaveData UseGameSaveData(int serialId)
            {
                if (serialId != 0)
                {
                    // 查找指定序列号的存档信息
                    GameSaveInfo saveInfo = m_GameSaveInfos.Find(info => info.SerialId == serialId);
                    LoadGameData(saveInfo.GetSaveDataFilePath());
                    m_PlayTime = saveInfo.PlayTime;
                    m_PlayerName = saveInfo.PlayerName;
                    m_PlayerLevel = saveInfo.PlayerLevel;
                    m_SceneName = saveInfo.SceneName;
                }
                else
                {
                    m_PlayTime = 0;
                    m_PlayerName = "PlayerUNKNOWN";
                    m_PlayerLevel = 0;
                    m_SceneName = "Init";
                }

                return m_CurrentSaveData;
            }

            private bool LoadGameData(string filePath)
            {
                try
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                        using (BinaryReader reader = new BinaryReader(fs))
                        {
                            // 读取哈希值
                            string storedHash = reader.ReadString();

                            // 读取加密数据
                            int encryptedDataLength = reader.ReadInt32();
                            byte[] encryptedData = reader.ReadBytes(encryptedDataLength);

                            // 解密数据
                            byte[] decryptedData = GameSaveSerializer.DecryptData(encryptedData);
                            if (decryptedData == null)
                            {
                                return false;
                            }

                            // 验证完整性
                            string calculatedHash = GameSaveSerializer.CalculateHash(decryptedData);
                            if (calculatedHash != storedHash)
                            {
                                Log.Warning("Save file integrity check failed for '{0}'.", filePath);
                                return false;
                            }

                            // 反序列化数据
                            if (!DeserializeGameDataFromBytes(decryptedData))
                            {
                                return false;
                            }
                        }

                    return true;
                }
                catch (Exception exception)
                {
                    Log.Error("Load game data failed with exception '{0}'.", exception);
                    return false;
                }
            }

            private byte[] SerializeGameDataToBytes()
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                        using (BinaryWriter writer = new BinaryWriter(ms))
                        {
                            if (m_CurrentSaveData != null)
                            {
                                m_CurrentSaveData.Serialize(writer);
                            }

                            return ms.ToArray();
                        }
                }
                catch (Exception exception)
                {
                    Log.Error("Serialize game data failed with exception '{0}'.", exception);
                    return null;
                }
            }

            private bool DeserializeGameDataFromBytes(byte[] data)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(data))
                        using (BinaryReader reader = new BinaryReader(ms))
                        {
                            if (m_CurrentSaveData == null)
                            {
                                m_CurrentSaveData = GameSaveData.CreateEmpty();
                            }

                            m_CurrentSaveData.Deserialize(reader);
                            return true;
                        }
                }
                catch (Exception exception)
                {
                    Log.Error("Deserialize game data failed with exception '{0}'.", exception);
                    return false;
                }
            }

            public bool IsValid()
            {
                return m_GameSaveId > 0 && !string.IsNullOrEmpty(m_GameSaveName);
            }

            private void CleanupOldSaves(string filePath)
            {
                try
                {
                    // 清理旧的存档信息文件
                    string[] infoFiles =
                        Directory.GetFiles(filePath, string.Format(GameSaveInfoFileName, m_GameSaveId, "*"));
                    if (infoFiles.Length > MaxSaveCount)
                    {
                        Array.Sort(infoFiles);
                        for (int i = 0; i < infoFiles.Length - MaxSaveCount; i++)
                        {
                            File.Delete(infoFiles[i]);
                        }
                    }

                    // 清理旧的存档数据文件
                    string[] dataFiles =
                        Directory.GetFiles(filePath, string.Format(GameSaveDataFileName, "*", "*"));
                    if (dataFiles.Length > MaxSaveCount)
                    {
                        Array.Sort(dataFiles);
                        for (int i = 0; i < dataFiles.Length - MaxSaveCount; i++)
                        {
                            File.Delete(dataFiles[i]);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("Cleanup old saves failed with exception '{0}'.", exception);
                }
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                m_PlayTime += realElapseSeconds;
            }

            /// <summary>
            /// 捕获屏幕截图
            /// </summary>
            /// <returns>屏幕截图的Sprite对象</returns>
            public static Sprite CaptureScreenShot()
            {
                try
                {
                    // 创建一个新的RenderTexture
                    RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
                    // 保存当前相机的渲染目标
                    Camera mainCamera = Camera.main;
                    RenderTexture prev = mainCamera.targetTexture;
                    // 设置新的渲染目标
                    mainCamera.targetTexture = rt;
                    // 渲染一帧
                    mainCamera.Render();
                    // 恢复原来的渲染目标
                    mainCamera.targetTexture = prev;

                    // 创建一个新的Texture2D
                    Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                    // 读取像素数据
                    RenderTexture.active = rt;
                    screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                    screenshot.Apply();
                    RenderTexture.active = null;

                    // 清理RenderTexture
                    rt.Release();
                    UnityEngine.Object.Destroy(rt);

                    // 创建Sprite
                    Sprite sprite = Sprite.Create(screenshot, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0.5f, 0.5f));
                    return sprite;
                }
                catch (Exception exception)
                {
                    Log.Error("Capture screenshot failed with exception '{0}'.", exception);
                    return null;
                }
            }
        }
    }
}