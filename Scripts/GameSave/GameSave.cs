using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityGameFramework.Runtime;

namespace LeeFramework.Scripts.GameSave
{
    public sealed partial class GameSave
    {
        private readonly SortedDictionary<int, GameSaveGroup> m_GameSaves = new SortedDictionary<int, GameSaveGroup>();
        private int m_CurrentGameSaveId;
        private bool m_IsGameStarted = false;
        private GameSaveData m_CurrentGameSaveData;

        /// <summary>
        /// 初始化游戏存档
        /// </summary>
        public GameSave()
        {
        }

        /// <summary>
        /// 获取游戏存档数量。
        /// </summary>
        public int Count
        {
            get
            {
                return m_GameSaves.Count;
            }
        }

        /// <summary>
        /// 获取所有游戏存档的信息。
        /// </summary>
        /// <returns>所有游戏存档的名称。</returns>
        public GameSaveInfo[] GetAllGameSaveInfos()
        {
            int index = 0;
            GameSaveInfo[] allGameSaveInfos = new GameSaveInfo[m_GameSaves.Count];
            foreach (KeyValuePair<int, GameSaveGroup> gameSave in m_GameSaves)
            {
                allGameSaveInfos[index++] = gameSave.Value.GetHeadInfo();
            }

            return allGameSaveInfos;
        }

        /// <summary>
        /// 检查是否存在指定游戏存档。
        /// </summary>
        /// <param name="gameSaveId">要检查游戏存档的Id。</param>
        /// <returns>指定的游戏存档是否存在。</returns>
        public bool HasGameSave(int gameSaveId)
        {
            return m_GameSaves.ContainsKey(gameSaveId);
        }

        /// <summary>
        /// 移除指定游戏存档。
        /// </summary>
        /// <param name="gameSaveId">要移除游戏存档的Id。</param>
        /// <returns>是否移除指定游戏存档成功。</returns>
        public bool RemoveGameSave(int gameSaveId)
        {
            return m_GameSaves.Remove(gameSaveId);
        }

        /// <summary>
        /// 清空所有游戏存档。
        /// </summary>
        public void RemoveAllGameSaves()
        {
            m_GameSaves.Clear();
        }

        /// <summary>
        /// 设置当前使用的存档
        /// </summary>
        /// <param name="saveInfo"></param>
        public void UseGameSave(int gameSaveId, int serialId)
        {
            m_CurrentGameSaveId = gameSaveId == 0 ? GenerateGameSaveId() : gameSaveId;
            GameSaveGroup group;
            if (!m_GameSaves.ContainsKey(m_CurrentGameSaveId))
            {
                group = GameSaveGroup.CreateWithGameSave(m_CurrentGameSaveId);
                m_GameSaves.Add(m_CurrentGameSaveId, group);
            }
            else
            {
                group = m_GameSaves[m_CurrentGameSaveId];
            }
            m_CurrentGameSaveData = group.UseGameSaveData(serialId);
            m_IsGameStarted = true;
        }

        public bool Save(string filePath)
        {
            try
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                if (m_GameSaves.TryGetValue(m_CurrentGameSaveId, out var group))
                {
                    return group.Save(Path.Combine(filePath, m_CurrentGameSaveId.ToString()));
                }

                return false;
            }
            catch (Exception exception)
            {
                Log.Error("Save game failed with exception '{0}'.", exception);
                return false;
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

                // 清空现有的存档数据
                m_GameSaves.Clear();

                // 获取所有存档组目录
                string[] directories = Directory.GetDirectories(gameSavePath);
                foreach (string directory in directories)
                {
                    // 从目录名中获取存档组ID
                    string directoryName = Path.GetFileName(directory);
                    if (int.TryParse(directoryName, out int gameSaveId))
                    {
                        // 创建存档组并加载
                        GameSaveGroup group = GameSaveGroup.CreateGameSaveFromFile(directory);
                        if (group != null)
                        {
                            // 将存档组添加到存档字典中
                            if (!m_GameSaves.ContainsKey(gameSaveId))
                            {
                                m_GameSaves.Add(gameSaveId, group);
                            }
                        }
                    }
                }

                // 如果有存档，设置当前存档为最新的存档
                if (m_GameSaves.Count > 0)
                {
                    m_CurrentGameSaveId = m_GameSaves.Keys.Max();
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Error("Load game failed with exception '{0}'.", exception);
                return false;
            }
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (m_IsGameStarted && m_GameSaves.TryGetValue(m_CurrentGameSaveId, out var group))
            {
                group.Update(elapseSeconds, realElapseSeconds);
            }
        }

        public static int GenerateGameSaveId()
        {
            // 基于当前时间戳生成一个唯一的存档ID
            int baseId = (int)(DateTime.UtcNow.Ticks % int.MaxValue);

            // 添加一个0-999的随机数，确保同一毫秒内的存档也有不同的ID
            int random = UnityEngine.Random.Range(0, 1000);

            // 组合时间戳和随机数
            return baseId + random;
        }
    }
}