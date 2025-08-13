using System.IO;

namespace LeeFramework.Scripts.Editor.DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class DoubleProcessor : DataTableProcessor.GenericDataProcessor<double>
        {
            public override bool IsSystem
            {
                get
                {
                    return true;
                }
            }

            public override string LanguageKeyword
            {
                get
                {
                    return "double";
                }
            }

            public override string[] GetTypeStrings()
            {
                return new string[]
                {
                    "double",
                    "system.double"
                };
            }

            public override double Parse(string value)
            {
                return double.Parse(value);
            }

            public override void WriteToStream(LeeFramework.Scripts.Editor.DataTableGenerator.DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}