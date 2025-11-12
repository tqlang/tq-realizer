using System.Drawing;
using System.Text;
using Tq.Realizer.Builder.References;
using Tq.Realizer.Core.Intermediate.Values;

namespace Tq.Realizer.Builder.ProgramMembers;

public class StaticFieldBuilder: FieldBuilder
{
    internal StaticFieldBuilder(INamespaceOrStructureBuilder parent, string name)
        : base(parent, name) {}


    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"(field static \"{Symbol}\"");
        
        //if (Offset != null) sb.Append($" (offset {Offset.Value})");
        //if (Alignment != null) sb.Append($" (alignment {Alignment.Value})");
        //if (Size != null) sb.Append($" (size {Size.Value})");
        
        sb.Append($" (type {Type?.ToString() ?? "<nil>"})");
        if (Initializer != null) sb.Append($" {Initializer}");
        sb.Append(')');

        return sb.ToString();
    }
    public override string ToReadableReference() => $"\"{string.Join('.', GlobalIdentifier)}\"";
}
