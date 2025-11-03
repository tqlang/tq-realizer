using System.Text;
using Tq.Realizer.Builder.References;

namespace Tq.Realizer.Builder.ProgramMembers;

public class InstanceFieldBuilder: FieldBuilder
{
    public TypeReference? Type { get; set; } = null;
    public uint? Offset { get; set; } = null;
    public uint? Alignment { get; set; } = null;
    public uint? Size { get; set; } = null;
    
    internal InstanceFieldBuilder(INamespaceOrStructureBuilder parent, string name, bool annonymous)
        : base(parent, name, annonymous) {}


    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"(field \"{Symbol}\"");
        
        if (Offset != null) sb.Append($" (offset {Offset.Value})");
        if (Alignment != null) sb.Append($" (alignment {Alignment.Value})");
        if (Size != null) sb.Append($" (size {Size.Value})");
        
        sb.Append($" (type {Type?.ToString() ?? "<nil>"})");
        sb.Append(')');

        return sb.ToString();
    }
    public override string ToReadableReference() => $"\"{string.Join('.', GlobalIdentifier)}\"";
}
