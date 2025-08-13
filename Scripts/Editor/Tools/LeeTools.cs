using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Path = System.IO.Path;

namespace LeeFramework.Scripts.Tools
{
    public static class Utility
    {
        /// <summary>
        /// 判断该文件是否是CSharp文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsCSharpFile(string fileName)
        {
            // 获取文件扩展名（包括点）
            string fileExtension = Path.GetExtension(fileName);

            // 将扩展名转换为小写并与 ".cs" 进行比较
            if (fileExtension.ToLower() == ".cs")
            {
                return true;
            }
            return false;
        }

        public static string GetFilePath()
        {
            Object selectedObject = Selection.activeObject;
            string result = "";
            if (selectedObject != null)
            {
                // 获取选定对象的相对路径
                string relativeAssetPath = AssetDatabase.GetAssetPath(selectedObject);
                // 获取项目根目录路径
                string projectPath = System.IO.Path.GetDirectoryName(Application.dataPath);
                // 获取选定对象的绝对路径
                string absoluteAssetPath = Path.Combine(projectPath, relativeAssetPath);
                // 获取选定对象的文件名（包括后缀）
                string fileName = Path.GetFileName(relativeAssetPath);

                //Debug.Log("执行自定义操作: " + selectedObject.name + ", 相对路径: " + relativeAssetPath + ", 绝对路径: " + absoluteAssetPath + ", 文件名: " + fileName);

                if (Utility.IsCSharpFile(fileName))
                {
                    result = absoluteAssetPath;
                }
                else
                {
                    Debug.LogWarning("File is Not CSharp File");
                }
            }
            else
            {
                Debug.LogWarning("Seleted Object is NULL!");
            }

            return result;
        }
    }

    /// <summary>
    /// 更改脚本编码格式
    /// </summary>
    public class ChangeScriptEncodingFormat
    {
        // 添加一个右键菜单。
        // % 按下ctrl时显示菜单。（Windows: control, macOS: command）
        // & 按下alt时显示菜单。(Windows/Linux: alt, macOS: option)
        // _ 按下shift时显示菜单。(Windows/Linux/macOS: shift)
        [MenuItem("Assets/Encoding Change : GB2312->UTF8", false, 100)]
        private static void CustomMenu()
        {
            string filepath = Utility.GetFilePath();
            if (!string.IsNullOrEmpty(filepath))
            {
                ChangeFormat(filepath);
            }
        }

        // 如果项目视图中有选中的对象，则启用右键菜单项
        [MenuItem("Assets/Encoding Change : GB2312->UTF8", true)]
        private static bool ValidateCustomMenu()
        {
            return Selection.activeObject != null;
        }

        /// <summary>
        /// 文件格式转码：GB2312转成UTF8
        /// 读取指定的文件，转换成UTF8（无BOM标记）格式后，回写覆盖原文件
        /// </summary>
        /// <param name="sourceFilePath">文件路径</param>
        public static void ChangeFormat(string sourceFilePath)
        {
            string fileContent = File.ReadAllText(sourceFilePath, Encoding.GetEncoding("GB2312"));
            File.WriteAllText(sourceFilePath, fileContent, Encoding.UTF8);
            Debug.Log("Encoding Change Finish!");
        }
    }

    /// <summary>
    /// 修改新建脚本中的指定信息
    /// </summary>
    public class AddScriptsInfo : UnityEditor.AssetModificationProcessor
    {
        private static string scriptName = "#SCRIPTNAME#"; //脚本名字
        private static string authorName = "#CreateAuthor#"; //作者名字
        private static string device = "#Device#"; //设备
        private static string email = "53033907+leexyy0804@users.noreply.github.com"; //"#Email#"; //邮箱
        private static string createTime = "#CreateTime#"; //创建时间
        private static string header =
            "//@LeeTools\n//------------------------\n";
        private static string fileDescribe =
            header +
            "//Filename：" + scriptName + "\n" +
            "//Author：" + authorName + "\n" +
            "//Device：" + device + "\n" +
            "//Email：" + email + "\n" +
            "//CreateDate：" + createTime + "\n" +
            "//Function：Nothing\n" +
            "//------------------------\n\n";

        /// <summary>
        /// 在创建资源的时候执行的函数
        /// </summary>
        /// <param name="path">脚本路径</param>
        private static void OnWillCreateAsset(string path)
        {
            if (!path.EndsWith(".cs.meta"))
            {
                return;
            }
            //将.meta文件屏蔽，避免获取到.meta文件
            path = path.Replace(".meta", "");

            //只对.cs文件操作
            if (path.EndsWith(".cs") == true)
            {
                addNoteToFile(path);
            }
        }

        [MenuItem("Assets/Note Scripts", false, 100)]
        public static void AddNote()
        {
            string filepath = Utility.GetFilePath();
            if (!string.IsNullOrEmpty(filepath))
            {
                addNoteToFile(filepath);
            }
        }

        [MenuItem("Assets/Note Scripts", true)]
        private static bool ValidateCustomMenu()
        {
            return Selection.activeObject != null;
        }

        private static void addNoteToFile(string path)
        {
            //文件名的分割获取
            string[] iterm = path.Split('/');

            string content = File.ReadAllText(path, Encoding.GetEncoding("GB2312"));
            if (content.StartsWith(header))
            {
                return;
            }
            //读取改路径该路径下的.cs文件中的所有脚本
            string str = fileDescribe + content;

            //进行关键字文件名，作者和时间获取并替换
            str = str.Replace(scriptName, iterm[iterm.Length - 1]).Replace(authorName, Environment.UserName).Replace(device, Environment.UserDomainName).Replace("#CreateTime#", string.Format("{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));

            //重新写入脚本
            File.WriteAllText(path, str, Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log("Scripts add Note Finish!");
        }
    }
}