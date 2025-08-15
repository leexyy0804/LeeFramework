using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFramework;
using LeeFramework.Scripts.Editor.DataTableGenerator.ExcelGenerator;
using LeeFramework.Scripts.Extensions;
using UnityEditor;
using UnityEngine;

namespace LeeFramework.Scripts.Editor.DataTableGenerator
{
    public sealed partial class DataTableGeneratorMenu : EditorWindow
    {
        /// <summary>
        /// 当前编辑器窗口实例
        /// </summary>
        private static DataTableGeneratorMenu _instance;

        /// <summary>
        /// Excel文件列表
        /// </summary>
        private static List<string> _excelList;

        /// <summary>
        /// 被选中的Excel文件列表
        /// </summary>
        private static List<string> _selectedExcelList;

        /// <summary>
        /// 已经导出的DataTable的名称
        /// </summary>
        private static HashSet<string> _dataTableNames;

        /// <summary>
        /// 滚动窗口初始位置
        /// </summary>
        private static Vector2 _scrollPos;

        /// <summary>
        /// 输出格式索引
        /// </summary>
        private static int _indexOfFormat;

        /// <summary>
        /// 输出格式
        /// </summary>
        private static readonly string[] FormatOption = { "CSV", "JSON", "XML", "TXT" };

        /// <summary>
        /// 编码索引
        /// </summary>
        private static int _indexOfEncoding;

        /// <summary>
        /// 编码选项
        /// </summary>
        private static readonly string[] EncodingOption = { "UTF-8", "GB2312" };

        /// <summary>
        /// 是否保留原始文件
        /// </summary>
        private static readonly bool KeepSource = true;

        private static readonly string ExcelPath = Path.Combine(Environment.CurrentDirectory, Path.Combine("Excel", "runtime"));

        private static readonly string
            OutputPath = Path.Combine(Application.dataPath, "GameMain/DataTable/Configs/");

        private static readonly string ExportedExcelListSavePath = Path.Combine(Application.dataPath, "GameMain/Configs/DataTableNames.txt");

        private static void Init()
        {
            //获取当前实例
            _instance = GetWindow<DataTableGeneratorMenu>();
            _excelList = new List<string>();
            _selectedExcelList = new List<string>();

            if (File.Exists(ExportedExcelListSavePath))
            {
                string data = File.ReadAllText(ExportedExcelListSavePath);
                _dataTableNames = new HashSet<string>(data.Split(","));
            }
            else
            {
                _dataTableNames = new HashSet<string>();
            }

            _scrollPos = new Vector2(_instance.position.x, _instance.position.y + 75);
        }

        private void OnGUI()
        {
            DrawOptions();
            DrawExport();
        }

        /// <summary>
        /// 绘制插件界面配置项
        /// </summary>
        private void DrawOptions()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("请选择格式类型:", GUILayout.Width(85));
            _indexOfFormat = EditorGUILayout.Popup(_indexOfFormat, FormatOption, GUILayout.Width(125));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("请选择编码类型:", GUILayout.Width(85));
            _indexOfEncoding = EditorGUILayout.Popup(_indexOfEncoding, EncodingOption, GUILayout.Width(125));
            GUILayout.EndHorizontal();

            //keepSource = GUILayout.Toggle(keepSource, "保留Excel源文件");
        }

        /// <summary>
        /// 绘制插件界面输出项
        /// </summary>
        private void DrawExport()
        {
            if (_excelList == null) return;
            if (_excelList.Count < 1)
            {
                EditorGUILayout.LabelField($"目录{ExcelPath}中没有可用excel文件！");
            }
            else
            {
                EditorGUILayout.LabelField("下列项目将被转换为" + FormatOption[_indexOfFormat] + ":");
                GUILayout.BeginVertical();
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true, GUILayout.Height(_excelList.Count * 18 > 200 ? 200 : _excelList.Count * 18));
                foreach (var s in _excelList)
                {
                    if (_dataTableNames.Contains(s.Split('.')[0]))
                    {
                        GUI.color = Color.green;
                    }
                    else
                    {
                        GUI.color = Color.red;
                    }
                    GUILayout.BeginHorizontal();
                    bool selected = _selectedExcelList.Contains(s);
                    selected = GUILayout.Toggle(selected, s);
                    if (selected && !_selectedExcelList.Contains(s))
                    {
                        _selectedExcelList.Add(s);
                    }
                    else if(!selected)
                    {
                        _selectedExcelList.Remove(s);
                    }
                    GUILayout.EndHorizontal();
                }

                GUI.color = Color.white;

                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                //输出
                if (GUILayout.Button("转换"))
                {
                    Convert();
                    _instance.Close();
                }

                if (GUILayout.Button("转换为bytes"))
                {
                    ConvertToBytes();
                }
            }
        }

        private static void ConvertToBytes()
        {
            //判断编码类型
            Encoding encoding = null;
            if (_indexOfEncoding == 0 || _indexOfEncoding == 3)
            {
                encoding = new UTF8Encoding(false);
            }
            else if (_indexOfEncoding == 1)
            {
                encoding = Encoding.GetEncoding("GB2312");
            }
            foreach (string dataTableName in _dataTableNames)
            {
                DataTableProcessor dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(dataTableName, encoding);
                if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                {
                    Debug.LogError(GameFramework.Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                    break;
                }

                DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
                DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 转换Excel文件
        /// </summary>
        private static void Convert()
        {
            if (_selectedExcelList == null || _selectedExcelList.Count == 0)
            {
                Debug.LogError("No excel selected!");
                return;
            }

            //判断编码类型
            Encoding encoding = null;
            if (_indexOfEncoding == 0 || _indexOfEncoding == 3)
            {
                encoding = new UTF8Encoding(false);
            }
            else if (_indexOfEncoding == 1)
            {
                encoding = Encoding.GetEncoding("GB2312");
            }

            //FileUtil.DeleteFileOrDirectory(exportedExcelListSavePath);
            //dataTableNames.Clear();

            foreach (var assetsPath in _selectedExcelList)
            {
                //获取Excel文件的绝对路径
                var path = ExcelPath + "/" + assetsPath;
                //构造Excel工具类
                var excel = new ExcelUtility(path);

                //判断输出类型
                if (_indexOfFormat == 0)
                {
                    var outputPath = Path.Join(OutputPath, "Csv");
                    if (!Directory.Exists(outputPath))
                    {
                        Directory.CreateDirectory(outputPath);
                    }

                    var output = Path.Combine(outputPath, assetsPath.Replace(".xlsx", ".csv"));
                    _dataTableNames.AddRange(excel.ConvertToCSV(output, encoding));
                }
                else if (_indexOfFormat == 1)
                {
                    var outputPath = Path.Join(OutputPath, "Json");
                    if (!Directory.Exists(outputPath))
                    {
                        Directory.CreateDirectory(outputPath);
                    }

                    var output = Path.Combine(outputPath, assetsPath.Replace(".xlsx", ".json"));
                    _dataTableNames.AddRange(excel.ConvertToJson(output, encoding));
                }
                else if (_indexOfFormat == 2)
                {
                    var outputPath = Path.Join(OutputPath, "Xml");
                    if (!Directory.Exists(outputPath))
                    {
                        Directory.CreateDirectory(outputPath);
                    }
                    
                    var output = Path.Combine(outputPath, assetsPath.Replace(".xlsx", ".xml"));
                    _dataTableNames.AddRange(excel.ConvertToXml(output));
                }
                else if (_indexOfFormat == 3)
                {
                    var outputPath = Path.Join(OutputPath, "Txt");
                    if (!Directory.Exists(outputPath))
                    {
                        Directory.CreateDirectory(outputPath);
                    }
                    
                    var output = Path.Combine(outputPath, assetsPath.Replace(".xlsx", ".txt"));
                    _dataTableNames.AddRange(excel.ConvertToTxt(output, encoding));
                }

                //判断是否保留源文件
                if (!KeepSource)
                {
                    FileUtil.DeleteFileOrDirectory(path);
                }

                //刷新本地资源
                AssetDatabase.Refresh();
            }

            //写入文件
            using var fileStream = new FileStream(ExportedExcelListSavePath, FileMode.Create, FileAccess.Write);
            using TextWriter textWriter = new StreamWriter(fileStream, encoding ?? new UTF8Encoding(false));
            textWriter.Write(string.Join(",", _dataTableNames));
        }

        /// <summary>
        /// 加载Excel
        /// </summary>
        private static void LoadExcel()
        {
            if (_excelList == null) _excelList = new List<string>();
            _excelList.Clear();

            var dir = new DirectoryInfo(ExcelPath);
            var fileInfo = dir.GetFileSystemInfos();
            foreach (var fileSystemInfo in fileInfo)
            {
                if (fileSystemInfo.Name.EndsWith(".xlsx"))
                {
                    _excelList.Add(fileSystemInfo.Name);
                }
            }

            foreach (var dataTableName in _dataTableNames)
            {
                if (!_excelList.Contains(dataTableName + ".xlsx"))
                {
                    _dataTableNames.Remove(dataTableName);
                }
            }
        }
    }
}