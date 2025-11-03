using System.Text;
using Tq.Realizer.Core.Intermediate.Language;
using Tq.Realizer.Builder.Language;
using Tq.Realizer.Builder.Language.Omega;
using Tq.Realizer.Builder.References;
using Tq.Realizer.Core.Intermediate.Values;

namespace Tq.Realizer.Builder.ProgramMembers;

public class FunctionBuilder: BaseFunctionBuilder
{
    public string? ExportSymbol = null;

    public bool IsInstance => Parameters.Count > 0 
                           && Parameters[0].name == "self"
                           && (Parameters[0].type is ReferenceTypeReference { Subtype: NodeTypeReference @ntr } 
                           && ntr.TypeReference == Parent);

    public readonly List<BlockBuilder> CodeBlocks = [];
    public readonly List<RealizerConstantValue> DataBlocks = [];
    
    internal FunctionBuilder(INamespaceOrStructureBuilder parent, string name, bool annonymous)
        : base(parent, name, annonymous) {}
    
    
    public OmegaBlockBuilder CreateOmegaBytecodeBlock(string name)
    {
        var realname = name;
        var i = 1;
        while (CodeBlocks.Any(e => e.Name == realname)) realname = name + i++;

        var block = new OmegaBlockBuilder(this, realname, (uint)CodeBlocks.Count);
        CodeBlocks.Add(block);
        return block;
    }

    
    public int AddDataBlock(RealizerConstantValue constant)
    {
        var index = DataBlocks.Count;
        DataBlocks.Add(constant);
        return index;
    }
    
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"(func \"{Symbol}\"");
        foreach (var (name, type) in Parameters) sb.Append($" (param \"{name}\" {type})");
        if (ReturnType != null) sb.Append($" (ret {ReturnType})");

        foreach (var builder in CodeBlocks)
        {
            sb.AppendLine($"\n\t(block ${builder.Name}");
            sb.Append($"{builder.DumpInstructionsToString().TabAllLines().TabAllLines()}");
            sb.AppendLine(")");
        }
        if (CodeBlocks.Count == 0) sb.Append("(no_body)");

        foreach (var (i, b) in DataBlocks.Index())
        {
            sb.Append($"\n(data ${i} {b})");
        }
        
        sb.AppendLine(")");
        return sb.ToString();
    }
    
}
