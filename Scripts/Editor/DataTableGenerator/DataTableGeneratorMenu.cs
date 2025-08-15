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
            if (File.Exists(ExportedExcelListSavePath))
            {
                string data = File.ReadAllText(ExportedExcelListSavePath);
                _dataTableNames = new HashSet<string>(data.Split(","));
            }
            else
            {
                _dataTableNames = new HashSet<string>();
            }
            ConvertToBytes();
        }

        /// <summary>
        /// 从excel导出为txt格式
        /// </summary>
        [MenuItem("First Battle/Generate DataTables/From Excel To Txt")]
        private static void GenerateDataTablesFromExcelToTxt()
        {
            if (File.Exists(ExportedExcelListSavePath))
            {
                string data = File.ReadAllText(ExportedExcelListSavePath);
                _dataTableNames = new HashSet<string>(data.Split(","));
            }
            else
            {
                _dataTableNames = new HashSet<string>();
            }

            LoadExcel();

            _indexOfFormat = 3;
            _indexOfEncoding = 0;
            if (_selectedExcelList == null) _selectedExcelList = new List<string>();
            _selectedExcelList.Clear();
            _selectedExcelList.AddRange(_excelList);
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
            _instance.Show();
        }
    }
}