using System.IO;

namespace LeeFramework.Scripts.Editor.DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class DecimalProcessor : DataTableProcessor.GenericDataProcessor<decimal>
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
                    return "decimal";
                }
            }

            public override string[] GetTypeStrings()
            {
                return new string[]
                {
                    "decimal",
                    "system.decimal"
                };
            }

            public override decimal Parse(string value)
            {
                return decimal.Parse(value);
            }

            public override void WriteToStream(LeeFramework.Scripts.Editor.DataTableGenerator.DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}