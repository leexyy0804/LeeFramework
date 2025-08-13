using System;
using System.Collections.Generic;
using System.IO;
using GameFramework;
using UnityGameFramework.Runtime;

namespace LeeFramework.Scripts.GameSave
{
    public sealed partial class GameSave
    {
        private sealed class GameSaveData
        {
            private const int Version = 1;
            private Dictionary<string, object> m_GameData = new Dictionary<string, object>();
            private Dictionary<string, byte[]> m_BinaryData = new Dictionary<string, byte[]>();

            private GameSaveData()
            {
            }

            public static GameSaveData CreateEmpty()
            {
                return new GameSaveData();
            }

            /// <summary>
            /// 序列化数据。
            /// </summary>
            /// <param name="stream">目标流。</param>
            /// <param name="writer"></param>
            public void Serialize(BinaryWriter writer)
            {
                try
                {
                    // 写入版本号
                    writer.Write(Version);

                    // 写入游戏数据
                    writer.Write(m_GameData.Count);
                    foreach (var kvp in m_GameData)
                    {
                        writer.Write(kvp.Key);
                        writer.Write(GameFramework.Utility.Json.ToJson(kvp.Value));
                        writer.Write(kvp.Value?.GetType().AssemblyQualifiedName ?? string.Empty);
                    }

                    // 写入二进制数据
                    writer.Write(m_BinaryData.Count);
                    foreach (var kvp in m_BinaryData)
                    {
                        writer.Write(kvp.Key);
                        writer.Write(kvp.Value.Length);
                        writer.Write(kvp.Value);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("Serialize GameSaveData failed with exception '{0}'.", exception);
                    throw;
                }
            }

            /// <summary>
            /// 反序列化数据。
            /// </summary>
            /// <param name="stream">指定流。</param>
            public void Deserialize(BinaryReader reader)
            {
                try
                {
                    // 读取版本号
                    int version = reader.ReadInt32();
                    if (version != Version)
                    {
                        throw new GameFrameworkException(
                            $"GameSaveData version mismatch. Current: {Version}, Saved: {version}");
                    }

                    // 读取游戏数据
                    int gameDataCount = reader.ReadInt32();
                    for (int i = 0; i < gameDataCount; i++)
                    {
                        string key = reader.ReadString();
                        string jsonValue = reader.ReadString();
                        string typeName = reader.ReadString();

                        if (!string.IsNullOrEmpty(typeName))
                        {
                            Type type = Type.GetType(typeName);
                            if (type != null)
                            {
                                m_GameData[key] = GameFramework.Utility.Json.ToObject(type, jsonValue);
                            }
                            else
                            {
                                Log.Warning($"Type '{typeName}' not found for key '{key}'.");
                                m_GameData[key] = null;
                            }
                        }
                        else
                        {
                            m_GameData[key] = null;
                        }
                    }

                    // 读取二进制数据
                    int binaryDataCount = reader.ReadInt32();
                    for (int i = 0; i < binaryDataCount; i++)
                    {
                        string key = reader.ReadString();
                        int dataLength = reader.ReadInt32();
                        byte[] data = reader.ReadBytes(dataLength);
                        m_BinaryData[key] = data;
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("Deserialize GameSaveData failed with exception '{0}'.", exception);
                    throw;
                }
            }

            public void SetData<T>(string key, T value)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new GameFrameworkException("Key is invalid.");
                }

                m_GameData[key] = value;
            }

            public T GetData<T>(string key, T defaultValue = default)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new GameFrameworkException("Key is invalid.");
                }

                if (m_GameData.TryGetValue(key, out object value))
                {
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }
                    else if (value != null)
                    {
                        try
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                        catch
                        {
                            Log.Warning(
                                $"Failed to convert value of type {value.GetType()} to {typeof(T)} for key '{key}'.");
                        }
                    }
                }

                return defaultValue;
            }

            public void SetBinaryData(string key, byte[] data)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new GameFrameworkException("Key is invalid.");
                }

                if (data == null)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                m_BinaryData[key] = data;
            }

            public byte[] GetBinaryData(string key)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new GameFrameworkException("Key is invalid.");
                }

                if (m_BinaryData.TryGetValue(key, out byte[] data))
                {
                    return data;
                }

                return null;
            }

            public void Clear()
            {
                m_GameData.Clear();
                m_BinaryData.Clear();
            }
        }
    }
}