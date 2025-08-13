using System.IO;

namespace LeeFramework.Scripts.Editor.DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class ByteProcessor : DataTableProcessor.GenericDataProcessor<byte>
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
                    return "byte";
                }
            }

            public override string[] GetTypeStrings()
            {
                return new string[]
                {
                    "byte",
                    "system.byte"
                };
            }

            public override byte Parse(string value)
            {
                return byte.Parse(value);
            }

            public override void WriteToStream(LeeFramework.Scripts.Editor.DataTableGenerator.DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}