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
        private static DataTableGeneratorMenu instance;

        /// <summary>
        /// Excel文件列表
        /// </summary>
        private static List<string> excelList;

        /// <summary>
        /// 被选中的Excel文件列表
        /// </summary>
        private static List<string> selectedExcelList;

        /// <summary>
        /// 已经导出的DataTable的名称
        /// </summary>
        private static HashSet<string> dataTableNames;

        /// <summary>
        /// 滚动窗口初始位置
        /// </summary>
        private static Vector2 scrollPos;

        /// <summary>
        /// 输出格式索引
        /// </summary>
        private static int indexOfFormat;

        /// <summary>
        /// 输出格式
        /// </summary>
        private static readonly string[] formatOption = { "CSV", "JSON", "XML", "TXT" };

        /// <summary>
        /// 编码索引
        /// </summary>
        private static int indexOfEncoding;

        /// <summary>
        /// 编码选项
        /// </summary>
        private static readonly string[] encodingOption = { "UTF-8", "GB2312" };

        /// <summary>
        /// 是否保留原始文件
        /// </summary>
        private static bool keepSource = true;

        private static readonly string excelPath = Path.Combine(Environment.CurrentDirectory, Path.Combine("Excel", "runtime"));

        private static readonly string
            outputPath = Path.Combine(Application.dataPath, "FirstBattle/GameMain/DataTables");

        private static readonly string
            ClassSavePath = Path.Combine(Application.dataPath, "FirstBattle/GameMain/Scripts/DataTables/");

        private static readonly string exportedExcelListSavePath = Path.Combine(Application.dataPath, "FirstBattle/GameMain/Configs/DataTableNames.txt");

        private static void Init()
        {
            //获取当前实例
            instance = GetWindow<DataTableGeneratorMenu>();
            excelList = new List<string>();
            selectedExcelList = new List<string>();

            if (File.Exists(exportedExcelListSavePath))
            {
                string data = File.ReadAllText(exportedExcelListSavePath);
                dataTableNames = new HashSet<string>(data.Split(","));
            }
            else
            {
                dataTableNames = new HashSet<string>();
            }

            scrollPos = new Vector2(instance.position.x, instance.position.y + 75);
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
            indexOfFormat = EditorGUILayout.Popup(indexOfFormat, formatOption, GUILayout.Width(125));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("请选择编码类型:", GUILayout.Width(85));
            indexOfEncoding = EditorGUILayout.Popup(indexOfEncoding, encodingOption, GUILayout.Width(125));
            GUILayout.EndHorizontal();

            //keepSource = GUILayout.Toggle(keepSource, "保留Excel源文件");
        }

        /// <summary>
        /// 绘制插件界面输出项
        /// </summary>
        private void DrawExport()
        {
            if (excelList == null) return;
            if (excelList.Count < 1)
            {
                EditorGUILayout.LabelField($"目录{excelPath}中没有可用excel文件！");
            }
            else
            {
                EditorGUILayout.LabelField("下列项目将被转换为" + formatOption[indexOfFormat] + ":");
                GUILayout.BeginVertical();
                scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(excelList.Count * 18 > 200 ? 200 : excelList.Count * 18));
                foreach (var s in excelList)
                {
                    if (dataTableNames.Contains(s.Split('.')[0]))
                    {
                        GUI.color = Color.green;
                    }
                    else
                    {
                        GUI.color = Color.red;
                    }
                    GUILayout.BeginHorizontal();
                    bool selected = selectedExcelList.Contains(s);
                    selected = GUILayout.Toggle(selected, s);
                    if (selected && !selectedExcelList.Contains(s))
                    {
                        selectedExcelList.Add(s);
                    }
                    else if(!selected)
                    {
                        selectedExcelList.Remove(s);
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
                    instance.Close();
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
            if (indexOfEncoding == 0 || indexOfEncoding == 3)
            {
                encoding = new UTF8Encoding(false);
            }
            else if (indexOfEncoding == 1)
            {
                encoding = Encoding.GetEncoding("GB2312");
            }
            foreach (string dataTableName in dataTableNames)
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
            if (selectedExcelList == null || selectedExcelList.Count == 0)
            {
                Debug.LogError("No excel selected!");
                return;
            }

            //判断编码类型
            Encoding encoding = null;
            if (indexOfEncoding == 0 || indexOfEncoding == 3)
            {
                encoding = new UTF8Encoding(false);
            }
            else if (indexOfEncoding == 1)
            {
                encoding = Encoding.GetEncoding("GB2312");
            }

            //FileUtil.DeleteFileOrDirectory(exportedExcelListSavePath);
            //dataTableNames.Clear();

            foreach (var assetsPath in selectedExcelList)
            {
                //获取Excel文件的绝对路径
                var path = excelPath + "/" + assetsPath;
                //构造Excel工具类
                var excel = new ExcelUtility(path);

                //判断输出类型
                var output = "";
                if (indexOfFormat == 0)
                {
                    output = Path.Join(outputPath, "Csv", assetsPath.Replace(".xlsx", ".csv"));
                    dataTableNames.AddRange(excel.ConvertToCSV(output, encoding));
                }
                else if (indexOfFormat == 1)
                {
                    output = Path.Join(outputPath, "Json", assetsPath.Replace(".xlsx", ".json"));
                    dataTableNames.AddRange(excel.ConvertToJson(output, encoding));
                }
                else if (indexOfFormat == 2)
                {
                    output = Path.Join(outputPath, "Xml", assetsPath.Replace(".xlsx", ".xml"));
                    dataTableNames.AddRange(excel.ConvertToXml(output));
                }
                else if (indexOfFormat == 3)
                {
                    output = Path.Join(outputPath, "Txt", assetsPath.Replace(".xlsx", ".txt"));
                    dataTableNames.AddRange(excel.ConvertToTxt(output, encoding));
                }

                //判断是否保留源文件
                if (!keepSource)
                {
                    FileUtil.DeleteFileOrDirectory(path);
                }

                //刷新本地资源
                AssetDatabase.Refresh();
            }

            //写入文件
            using var fileStream = new FileStream(exportedExcelListSavePath, FileMode.Create, FileAccess.Write);
            using TextWriter textWriter = new StreamWriter(fileStream, encoding ?? new UTF8Encoding(false));
            textWriter.Write(string.Join(",", dataTableNames));
        }

        /// <summary>
        /// 加载Excel
        /// </summary>
        private static void LoadExcel()
        {
            if (excelList == null) excelList = new List<string>();
            excelList.Clear();

            var dir = new DirectoryInfo(excelPath);
            var fileInfo = dir.GetFileSystemInfos();
            foreach (var fileSystemInfo in fileInfo)
            {
                if (fileSystemInfo.Name.EndsWith(".xlsx"))
                {
                    excelList.Add(fileSystemInfo.Name);
                }
            }

            foreach (var dataTableName in dataTableNames)
            {
                if (!excelList.Contains(dataTableName + ".xlsx"))
                {
                    dataTableNames.Remove(dataTableName);
                }
            }
        }
    }
}