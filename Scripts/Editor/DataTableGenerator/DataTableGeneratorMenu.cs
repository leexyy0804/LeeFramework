using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace LeeFramework.Scripts.Editor.DataTableGenerator
{
    public sealed partial class DataTableGeneratorMenu
    {
        /// <summary>
        /// 从Txt导出为最终格式
        /// </summary>
        [MenuItem("First Battle/Generate DataTables/From Txt To Bytes")]
        private static void GenerateDataTablesFromTxtToBytes()
        {
            if (File.Exists(exportedExcelListSavePath))
            {
                string data = File.ReadAllText(exportedExcelListSavePath);
                dataTableNames = new HashSet<string>(data.Split(","));
            }
            else
            {
                dataTableNames = new HashSet<string>();
            }
            ConvertToBytes();
        }

        /// <summary>
        /// 从excel导出为txt格式
        /// </summary>
        [MenuItem("First Battle/Generate DataTables/From Excel To Txt")]
        private static void GenerateDataTablesFromExcelToTxt()
        {
            if (File.Exists(exportedExcelListSavePath))
            {
                string data = File.ReadAllText(exportedExcelListSavePath);
                dataTableNames = new HashSet<string>(data.Split(","));
            }
            else
            {
                dataTableNames = new HashSet<string>();
            }

            LoadExcel();

            indexOfFormat = 3;
            indexOfEncoding = 0;
            if (selectedExcelList == null) selectedExcelList = new List<string>();
            selectedExcelList.Clear();
            selectedExcelList.AddRange(excelList);
            Convert();
        }

        /// <summary>
        /// 显示当前窗口
        /// </summary>
        [MenuItem("First Battle/Generate DataTables/Editor Window")]
        private static void ShowExcelTools()
        {
            Init();
            //加载Excel文件
            LoadExcel();
            instance.Show();
        }
    }
}