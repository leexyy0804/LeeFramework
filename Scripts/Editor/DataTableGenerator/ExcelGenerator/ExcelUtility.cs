using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ExcelDataReader;
using LitJson;
using UnityEngine;

namespace LeeFramework.Scripts.Editor.DataTableGenerator.ExcelGenerator
{
    public class ExcelUtility
    {
        /// <summary>
        /// 表格数据集合
        /// </summary>
        private DataSet mResultSet;

        private string xmlPath = Application.dataPath + "/xmlName.xml";

        private string[] fName;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="excelFile">Excel file.</param>
        public ExcelUtility(string excelFile)
        {
            using var mStream = File.Open(excelFile, FileMode.Open, FileAccess.Read);
            var mExcelReader = ExcelReaderFactory.CreateOpenXmlReader(mStream);
            mResultSet = mExcelReader.AsDataSet();
        }

        /// <summary>
        /// 转换为Json
        /// </summary>
        /// <param name="JsonPath">Json文件路径</param>
        /// <param name="Header">表头行数</param>
        public List<string> ConvertToJson(string JsonPath, Encoding encoding)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(JsonPath))
            {
                return result;
            }

            //判断Excel文件中是否存在数据表
            if (mResultSet.Tables.Count < 1)
                return result;

            var jsonFileName = Path.GetFileName(JsonPath);
            var tableName = jsonFileName.Remove(jsonFileName.IndexOf(".", StringComparison.Ordinal));

            //默认读取第一个数据表
            foreach (System.Data.DataTable mSheet in mResultSet.Tables)
            {
                //判断数据表内是否存在数据
                if (mSheet.Rows.Count < 1)
                    continue;

                if (mResultSet.Tables.Count > 1)
                {
                    JsonPath = JsonPath.Replace(tableName, mSheet.TableName);
                    tableName = mSheet.TableName;
                }

                result.Add(tableName);

                //读取数据表行数和列数
                var rowCount = mSheet.Rows.Count;
                var colCount = mSheet.Columns.Count;

                //准备一个列表存储整个表的数据
                var table = new List<Dictionary<string, object>>();

                //读取数据
                for (var i = 3; i < rowCount; i++)
                {
                    //准备一个字典存储每一行的数据
                    var row = new Dictionary<string, object>();
                    for (var j = 0; j < colCount; j++)
                    {
                        //读取第1行数据作为表头字段
                        var field = mSheet.Rows[1][j].ToString();
                        if (string.IsNullOrEmpty(field))
                            continue;
                        //Key-Value对应
                        row[field] = mSheet.Rows[i][j];
                    }

                    //添加到表数据中
                    table.Add(row);
                }

                //生成Json字符串
                //string json = JsonUtility.ToJson(table);
                // var s1 = new JsonSerializerSettings();
                // s1.Converters.Add(new DemoJson());
                //
                // var json = JsonConvert.SerializeObject(table, Formatting.Indented, s1);

                string json = JsonMapper.ToJson(table);

                //写入文件
                using var fileStream = new FileStream(JsonPath, FileMode.Create, FileAccess.Write);
                using TextWriter textWriter = new StreamWriter(fileStream, encoding);
                textWriter.Write(json);
            }

            return result;
        }

        /// <summary>
        /// 转换为txt
        /// </summary>
        /// <param name="savePath">文件保存路径</param>
        /// <param name="encoding"></param>
        public List<string> ConvertToTxt(string savePath, Encoding encoding)
        {
            List<string> result = new List<string>();
            //判断Excel文件中是否存在数据表
            if (mResultSet.Tables.Count < 1)
                return result;

            var stringBuilder = new StringBuilder();

            //获取表的名称
            var fileName = Path.GetFileName(savePath);
            var tableName = fileName.Remove(fileName.IndexOf(".", StringComparison.Ordinal));

            //读取数据表
            foreach (System.Data.DataTable mSheet in mResultSet.Tables)
            {
                stringBuilder.Length = 0;
                //判断数据表内是否存在数据
                if (mSheet.Rows.Count < 1)
                    continue;
                if (mResultSet.Tables.Count > 1)
                {
                    savePath = savePath.Replace(tableName, mSheet.TableName);
                    tableName = mSheet.TableName;
                }

                result.Add(tableName);

                //读取数据表行数和列数
                var rowCount = mSheet.Rows.Count;
                var colCount = mSheet.Columns.Count;

                char splitter = '\t';

                stringBuilder.Append("#");
                for (var j = 0; j < colCount; j++)
                {
                    if (j == 0)
                    {
                        stringBuilder.Append($"{splitter}{tableName}");
                    }
                    else
                    {
                        stringBuilder.Append($"{splitter}");
                    }
                }
                stringBuilder.Append("\n");
                for (var i = 0; i < rowCount; i++)
                {
                    if (i < 3)
                    {
                        stringBuilder.Append('#');
                    }

                    for (var j = 0; j < colCount; j++)
                    {
                        var field = mSheet.Rows[i][j].ToString() ?? "";
                        stringBuilder.Append($"{splitter}{field}");
                    }

                    stringBuilder.Append("\n");
                }

                //写入文件
                using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write);
                using TextWriter textWriter = new StreamWriter(fileStream, encoding);
                textWriter.Write(stringBuilder.ToString());
            }

            return result;
        }

        /// <summary>
        /// 转换为CSV
        /// </summary>
        public List<string> ConvertToCSV(string savePath, Encoding encoding)
        {
            List<string> result = new List<string>();
            //判断Excel文件中是否存在数据表
            if (mResultSet.Tables.Count < 1)
                return result;

            var stringBuilder = new StringBuilder();

            //获取表的名称
            var fileName = Path.GetFileName(savePath);
            var tableName = fileName.Remove(fileName.IndexOf(".", StringComparison.Ordinal));

            //读取数据表
            foreach (System.Data.DataTable mSheet in mResultSet.Tables)
            {
                stringBuilder.Length = 0;
                //判断数据表内是否存在数据
                if (mSheet.Rows.Count < 1)
                    continue;
                if (mResultSet.Tables.Count > 1)
                {
                    savePath = savePath.Replace(tableName, mSheet.TableName);
                    tableName = mSheet.TableName;
                }

                result.Add(tableName);

                //读取数据表行数和列数
                var rowCount = mSheet.Rows.Count;
                var colCount = mSheet.Columns.Count;

                //读取数据
                for (var i = 1; i < rowCount; i++)
                {
                    for (var j = 0; j < colCount; j++)
                    {
                        //使用","分割每一个数值
                        stringBuilder.Append(mSheet.Rows[i][j] + ",");
                    }

                    //使用换行符分割每一行
                    stringBuilder.Append("\r\n");
                }

                //写入文件
                using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                {
                    using (TextWriter textWriter = new StreamWriter(fileStream, encoding))
                    {
                        textWriter.Write(stringBuilder.ToString());
                    }
                }
            }

            return result;
        }

        private List<string> mList = new List<string>();
        private List<string> sList = new List<string>();

        public void ReadXmlName()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            var rootlist = xmlDoc.GetElementsByTagName("root")[0].ChildNodes.Count;

            var nodeList = xmlDoc.SelectSingleNode("root").ChildNodes;

            for (var k = 0; k < rootlist; k++)
            {
                mList.Add(xmlDoc.GetElementsByTagName("root")[0].ChildNodes[k].Name);
            }


            foreach (XmlElement xe in nodeList)
            {
                foreach (XmlElement x1 in xe.ChildNodes)
                {
                    sList.Add(x1.InnerText);
                }
            }
        }

        /// <summary>
        /// 导出为Xml
        /// </summary>
        public List<string> ConvertToXml(string savePath)
        {
            List<string> result = new List<string>();
            //ReadXmlName();
            //判断Excel文件中是否存在数据表
            if (mResultSet.Tables.Count < 1)
                return result;

            //创建一个StringBuilder存储数据
            var stringBuilder = new StringBuilder();

            //获取表的名称
            var fileName = Path.GetFileName(savePath);
            var tableName = fileName.Remove(fileName.IndexOf(".", StringComparison.Ordinal));
            //读取数据表
            foreach (System.Data.DataTable mSheet in mResultSet.Tables)
            {
                stringBuilder.Length = 0;
                //判断数据表内是否存在数据
                if (mSheet.Rows.Count < 1)
                    continue;
                if (mResultSet.Tables.Count > 1)
                {
                    savePath = savePath.Replace(tableName, mSheet.TableName);
                    tableName = mSheet.TableName;
                }

                result.Add(tableName);

                var Zz = @"[\u4e00-\u9fa5]";
                //判断数据表内是否存在数据
                if (mSheet.Rows.Count < 1)
                    return result;

                //读取数据表行数和列数
                var rowCount = mSheet.Rows.Count;
                var colCount = mSheet.Columns.Count;

                //创建Xml文件头
                stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                stringBuilder.Append("\r\n");
                //创建根节点
                stringBuilder.Append("<root xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
                stringBuilder.Append("\r\n");

                //读取数据
                for (var i = 1; i < rowCount; i++)
                {
                    //创建子节点
                    for (var a = 0; a < mList.Count; a++)
                    {
                        if (tableName == mList[a])
                        {
                            stringBuilder.Append("\t<" + sList[a] + ">");
                            stringBuilder.Append("\r\n");
                        }
                    }


                    for (var j = 0; j < colCount; j++)
                    {
                        if (Regex.IsMatch(mSheet.Rows[0][j].ToString(), Zz) ||
                            string.IsNullOrEmpty(mSheet.Rows[0][j].ToString()))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(mSheet.Rows[i][j].ToString()) && tableName.Contains("Name"))
                        {
                            stringBuilder.Append("<" + mSheet.Rows[0][j] + "/>");
                            stringBuilder.Append("\r\n");
                            continue;
                        }

                        if (string.IsNullOrEmpty(mSheet.Rows[i][j].ToString()))
                        {
                            continue;
                        }

                        stringBuilder.Append("\t\t<" + mSheet.Rows[0][j] + ">");
                        stringBuilder.Append(mSheet.Rows[i][j]);
                        stringBuilder.Append("</" + mSheet.Rows[0][j] + ">");
                        stringBuilder.Append("\r\n");
                    }

                    //使用换行符分割每一行
                    for (var a = 0; a < mList.Count; a++)
                    {
                        if (tableName == mList[a])
                        {
                            stringBuilder.Append("\t</" + sList[a] + ">");
                            stringBuilder.Append("\r\n");
                        }
                    }
                }

                //闭合标签
                stringBuilder.Append("</root>");
                //写入文件
                using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                {
                    using (TextWriter textWriter = new StreamWriter(fileStream, Encoding.GetEncoding("utf-8")))
                    {
                        textWriter.Write(stringBuilder.ToString());
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 设置目标实例的属性
        /// </summary>
        private void SetTargetProperty(object target, string propertyName, object propertyValue)
        {
            //获取类型
            var mType = target.GetType();
            //获取属性集合
            var mPropertys = mType.GetProperties();
            foreach (var property in mPropertys)
            {
                if (property.Name == propertyName)
                {
                    property.SetValue(target, Convert.ChangeType(propertyValue, property.PropertyType), null);
                }
            }
        }

        /// <summary>
        /// 读取excel表格内容(函数重载形成不同)
        /// </summary>
        /// <param name="row">查找列值</param>
        /// <param name="col">结果列键</param>
        /// <returns></returns>
        public string ReadExcelContent(string colKey, string row, string col)
        {
            var table = mResultSet.Tables[0];
            var colNum = 0;
            for (var i = 0; i < table.Columns.Count; i++)
            {
                if (table.Rows[0][i].ToString() == col)
                {
                    colNum = i;
                }
            }

            var colKeyNum = 0;
            for (var i = 0; i < table.Columns.Count; i++)
            {
                if (table.Rows[0][i].ToString() == colKey)
                {
                    colKeyNum = i;
                }
            }

            for (var i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][colKeyNum].ToString() == row)
                {
                    Debug.Log(table.Rows[i][colNum]);
                    return table.Rows[i][colNum].ToString();
                }
            }

            return null;
        }
    }
}
//
// public class DemoJson : JsonConverter
// {
//     private void dumpNumArray<T>(JsonWriter writer, T n)
//     {
//         var s = n.ToString();
//         if (s.EndsWith(".0"))
//             writer.WriteRawValue(s.Substring(0, s.Length - 2));
//         else if (s.Contains("."))
//             writer.WriteRawValue(s.TrimEnd('0').TrimEnd('.'));
//         else
//             writer.WriteRawValue(s);
//     }
//
//     public override void WriteJson(JsonWriter writer, object value,
//         JsonSerializer serializer)
//     {
//         var t = value.GetType();
//
//         if (t == dblArrayType)
//             dumpNumArray(writer, (double)value);
//         else if (t == decArrayType)
//             dumpNumArray(writer, (decimal)value);
//         else
//             throw new NotImplementedException();
//     }
//
//     private Type dblArrayType = typeof(double);
//     private Type decArrayType = typeof(decimal);
//
//     public override bool CanConvert(Type objectType)
//     {
//         if (objectType == dblArrayType || objectType == decArrayType)
//             return true;
//         return false;
//     }
//
//     public override bool CanRead => false;
//
//     public override object ReadJson(JsonReader reader, Type objectType,
//         object existingValue, JsonSerializer serializer)
//     {
//         throw new NotImplementedException();
//     }
// }