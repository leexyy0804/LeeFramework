using System;
using System.Collections.Generic;
using System.IO;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;
using System.Linq;

namespace FirstBattle
{
    public class GameSaveComponent : GameFrameworkComponent
    {
        private const string GameSaveDataDirectoryName = "GameSaveData";
        private const float AutoSaveInterval = 300f; // 5分钟自动保存一次
        private const string BackupDirectoryName = "Backup";
        private const int MaxBackupCount = 5;

        private string m_FilePath = null;
        private GameSave m_GameSaves = null;
        private GameSaveSerializer m_Serializer = null;
        private float m_AutoSaveTimer = 0f;
        private bool m_IsAutoSaveEnabled = true;

        /// <summary>
        /// 获取游戏存档数量。
        /// </summary>
        public int Count
        {
            get
            {
                return m_GameSaves != null ? m_GameSaves.Count : 0;
            }
        }

        /// <summary>
        /// 获取游戏存档存储文件路径。
        /// </summary>
        public string FilePath
        {
            get
            {
                return m_FilePath;
            }
        }

        /// <summary>
        /// 获取游戏存档序列化器。
        /// </summary>
        public GameSaveSerializer Serializer
        {
            get
            {
                return m_Serializer;
            }
        }

        /// <summary>
        /// 加载游戏存档。
        /// </summary>
        /// <returns>是否加载游戏存档成功。</returns>
        public bool Load()
        {
            try
            {
                if (!Directory.Exists(m_FilePath))
                {
                    return true;
                }

                m_GameSaves.Load(m_FilePath);

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Load settings failure with exception '{0}'.", exception);
                return false;
            }
        }

        /// <summary>
        /// 保存游戏存档。
        /// </summary>
        /// <returns>是否保存游戏存档成功。</returns>
        public bool Save()
        {
            try
            {
                return m_GameSaves.Save(m_FilePath);
            }
            catch (Exception exception)
            {
                Log.Warning("Save settings failure with exception '{0}'.", exception);
                return false;
            }
        }

        /// <summary>
        /// 获取所有游戏存档的名称。
        /// </summary>
        /// <returns>所有游戏存档的名称。</returns>
        public GameSaveInfo[] GetAllGameSaveInfos()
        {
            return m_GameSaves.GetAllGameSaveInfos();
        }

        public void Update()
        {
            m_GameSaves.Update(Time.deltaTime, Time.unscaledDeltaTime);
            
            if (m_IsAutoSaveEnabled)
            {
                m_AutoSaveTimer += Time.unscaledDeltaTime;
                if (m_AutoSaveTimer >= AutoSaveInterval)
                {
                    m_AutoSaveTimer = 0f;
                    AutoSave();
                }
            }
        }

        /// <summary>
        /// 启用或禁用自动保存功能
        /// </summary>
        /// <param name="enabled">是否启用自动保存</param>
        public void SetAutoSaveEnabled(bool enabled)
        {
            m_IsAutoSaveEnabled = enabled;
            if (!enabled)
            {
                m_AutoSaveTimer = 0f;
            }
        }

        /// <summary>
        /// 执行自动保存
        /// </summary>
        private void AutoSave()
        {
            try
            {
                if (m_GameSaves != null)
                {
                    Save();
                    Log.Info("Auto save completed successfully.");
                }
            }
            catch (Exception exception)
            {
                Log.Warning("Auto save failed with exception '{0}'.", exception);
            }
        }

        /// <summary>
        /// 获取所有游戏存档的名称。
        /// </summary>
        /// <param name="results">所有游戏存档的名称。</param>
        // public void GetAllGameSaveNames(List<string> results)
        // {
        //     m_GameSaves.GetAllGameSaveNames(results);
        // }

        /// <summary>
        /// 检查是否存在指定游戏存档。
        /// </summary>
        /// <param name="gameSaveName">要检查游戏存档的名称。</param>
        /// <returns>指定的游戏存档是否存在。</returns>
        // public bool HasGameSave(string gameSaveName)
        // {
        //     return m_GameSaves.HasGameSave(gameSaveName);
        // }

        /// <summary>
        /// 移除指定游戏存档。
        /// </summary>
        /// <param name="gameSaveName">要移除游戏存档的名称。</param>
        /// <returns>是否移除指定游戏存档成功。</returns>
        // public bool RemoveGameSave(string gameSaveName)
        // {
        //     return m_GameSaves.RemoveGameSave(gameSaveName);
        // }

        /// <summary>
        /// 清空所有游戏存档。
        /// </summary>
        public void RemoveAllSave()
        {
            m_GameSaves.RemoveAllGameSaves();
        }

        public void UseGameSave(GameSaveInfo gameSaveInfo)
        {
            int gameSaveId = gameSaveInfo != null ? gameSaveInfo.GameSaveId : 0;
            int serialId = gameSaveInfo != null ? gameSaveInfo.SerialId : 0;

            m_GameSaves.UseGameSave(gameSaveId, serialId);
        }

        /// <summary>
        /// 创建存档备份
        /// </summary>
        public bool CreateBackup()
        {
            try
            {
                string backupPath = Path.Combine(m_FilePath, BackupDirectoryName);
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                // 创建备份文件名
                string backupFileName = string.Format("Backup_{0}.dat", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string backupFilePath = Path.Combine(backupPath, backupFileName);

                // 复制当前存档到备份目录
                if (File.Exists(m_FilePath))
                {
                    File.Copy(m_FilePath, backupFilePath, true);
                }

                // 清理旧备份
                CleanupOldBackups(backupPath);

                return true;
            }
            catch (Exception exception)
            {
                Log.Error("Create backup failed with exception '{0}'.", exception);
                return false;
            }
        }

        /// <summary>
        /// 从备份恢复存档
        /// </summary>
        /// <param name="backupFileName">备份文件名</param>
        public bool RestoreFromBackup(string backupFileName)
        {
            try
            {
                string backupPath = Path.Combine(m_FilePath, BackupDirectoryName);
                string backupFilePath = Path.Combine(backupPath, backupFileName);

                if (!File.Exists(backupFilePath))
                {
                    Log.Error("Backup file '{0}' not found.", backupFilePath);
                    return false;
                }

                // 创建当前存档的备份
                string currentBackupPath = Path.Combine(backupPath, string.Format("PreRestore_{0}.dat", DateTime.Now.ToString("yyyyMMdd_HHmmss")));
                if (File.Exists(m_FilePath))
                {
                    File.Copy(m_FilePath, currentBackupPath, true);
                }

                // 恢复备份
                File.Copy(backupFilePath, m_FilePath, true);

                // 重新加载存档
                return Load();
            }
            catch (Exception exception)
            {
                Log.Error("Restore from backup failed with exception '{0}'.", exception);
                return false;
            }
        }

        /// <summary>
        /// 获取所有可用的备份
        /// </summary>
        public string[] GetAvailableBackups()
        {
            try
            {
                string backupPath = Path.Combine(m_FilePath, BackupDirectoryName);
                if (!Directory.Exists(backupPath))
                {
                    return new string[0];
                }

                return Directory.GetFiles(backupPath, "Backup_*.dat")
                    .Select(Path.GetFileName)
                    .OrderByDescending(f => f)
                    .ToArray();
            }
            catch (Exception exception)
            {
                Log.Error("Get available backups failed with exception '{0}'.", exception);
                return new string[0];
            }
        }

        /// <summary>
        /// 清理旧的备份文件
        /// </summary>
        private void CleanupOldBackups(string backupPath)
        {
            try
            {
                var backupFiles = Directory.GetFiles(backupPath, "Backup_*.dat")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                while (backupFiles.Count > MaxBackupCount)
                {
                    var oldestBackup = backupFiles.Last();
                    oldestBackup.Delete();
                    backupFiles.RemoveAt(backupFiles.Count - 1);
                }
            }
            catch (Exception exception)
            {
                Log.Error("Cleanup old backups failed with exception '{0}'.", exception);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            m_FilePath = Utility.Path.GetRegularPath(Path.Combine(Application.persistentDataPath,  GameSaveDataDirectoryName));
            m_GameSaves = new GameSave();
            m_Serializer = new GameSaveSerializer();
            //m_Serializer.RegisterSerializeCallback(0, SerializeGameSaveCallback);
            //m_Serializer.RegisterDeserializeCallback(0, DeserializeGameSaveCallback);

            Load();
        }

        // private bool SerializeGameSaveCallback(Stream stream, GameSave defaultSetting)
        // {
        //     m_GameSaves.Serialize(stream);
        //     return true;
        // }

        // private GameSave DeserializeGameSaveCallback(Stream stream)
        // {
        //     m_GameSaves.Deserialize(stream);
        //     return m_GameSaves;
        // }
    }
}