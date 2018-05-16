using System.Collections.Generic;
using System.Text;

namespace WinCompData.CodeGen
{
#if !WINDOWS_UWP
    public
#endif
    sealed class CodeBuilder
    {
        const int c_indentSize = 4;
        readonly List<CodeLine> _lines = new List<CodeLine>();
        int _indentCount = 0;

        internal bool IsEmpty => _lines.Count == 0;

        internal void WriteLine()
        {
            WriteLine("");
        }

        internal void WriteLine(string line)
        {
            _lines.Add(new CodeLine { Text = line, IndentCount = _indentCount });
        }

        internal void WriteCodeBuilder(CodeBuilder builder)
        {
            _lines.Add(new CodeLine { Text = builder, IndentCount = _indentCount });
        }

        internal void OpenScope()
        {
            WriteLine("{");
            Indent();
        }

        internal void CloseScope()
        {
            UnIndent();
            WriteLine("}");
        }

        internal void CloseClassScope()
        {
            UnIndent();
            WriteLine("};");
        }

        internal void Indent()
        {
            _indentCount++;
        }

        internal void UnIndent()
        {
            _indentCount--;
        }

        internal void Clear()
        {
            _lines.Clear();
        }

        public override string ToString()
        {
            return ToString(0);
        }

        internal string ToString(int indentCount)
        {
            var sb = new StringBuilder();
            foreach (var line in _lines)
            {
                var builder = line.Text as CodeBuilder;
                if (builder != null)
                {
                    sb.Append(builder.ToString(line.IndentCount + indentCount));
                }
                else
                {
                    sb.Append(new string(' ', (line.IndentCount + indentCount) * c_indentSize));
                    sb.AppendLine(line.Text.ToString());
                }
            }

            return sb.ToString();
        }

        struct CodeLine
        {
            // A string or a CodeBuilder.
            internal object Text;
            internal int IndentCount;
        }
    }
}
