using System.IO;
using UnityEngine;

namespace LeeFramework.Scripts.Editor.DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class ColorProcessor : DataTableProcessor.GenericDataProcessor<Color>
        {
            public override bool IsSystem
            {
                get
                {
                    return false;
                }
            }

            public override string LanguageKeyword
            {
                get
                {
                    return "Color";
                }
            }

            public override string[] GetTypeStrings()
            {
                return new string[]
                {
                    "color",
                    "unityengine.color"
                };
            }

            public override Color Parse(string value)
            {
                string[] splitedValue = value.Split(',');
                return new Color(float.Parse(splitedValue[0]), float.Parse(splitedValue[1]), float.Parse(splitedValue[2]), float.Parse(splitedValue[3]));
            }

            public override void WriteToStream(LeeFramework.Scripts.Editor.DataTableGenerator.DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                Color color = Parse(value);
                binaryWriter.Write(color.r);
                binaryWriter.Write(color.g);
                binaryWriter.Write(color.b);
                binaryWriter.Write(color.a);
            }
        }
    }
}