using System.Text;

namespace LeeFramework.Scripts.Editor.DataTableGenerator
{
    public delegate void DataTableCodeGenerator(LeeFramework.Scripts.Editor.DataTableGenerator.DataTableProcessor dataTableProcessor, StringBuilder codeContent, object userData);
}