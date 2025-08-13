using System.IO;

namespace LeeFramework.Scripts.Editor.DataTableGenerator
{
    public sealed partial class DataTableProcessor
    {
        private sealed class CharProcessor : DataTableProcessor.GenericDataProcessor<char>
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
                    return "char";
                }
            }

            public override string[] GetTypeStrings()
            {
                return new string[]
                {
                    "char",
                    "system.char"
                };
            }

            public override char Parse(string value)
            {
                return char.Parse(value);
            }

            public override void WriteToStream(LeeFramework.Scripts.Editor.DataTableGenerator.DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}