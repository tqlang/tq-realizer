using System.Text;

namespace Tq.Realizer.Builder.ProgramMembers;

public class VirtualFunctionBuilder: FunctionBuilder
{
    public uint Index = 0;

    internal VirtualFunctionBuilder(INamespaceOrStructureBuilder parent, string name, uint index, bool annonymous)
        : base(parent, name, annonymous)
    {
        Index = index;
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"(func (virtual {Index}) \"{Symbol}\"");
        foreach (var (name, type) in Parameters) sb.Append($" (param \"{name}\" {type})");
        if (ReturnType != null) sb.Append($" (ret {ReturnType})");
        
        foreach (var builder in CodeBlocks)
        {
            sb.AppendLine($"\n\t{builder.Name}:");
            sb.AppendLine($"{builder.DumpInstructionsToString().TabAllLines().TabAllLines()})");
        }
        if (CodeBlocks.Count == 0) sb.Append("(no_body)");
        
        return sb.ToString();
    }
}
