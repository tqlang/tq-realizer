using Tq.Realizer.Builder.ProgramMembers;

namespace Tq.Realizer.Builder.Language;

public abstract class BlockBuilder(FunctionBuilder p, string n, uint idx)
{
    public readonly FunctionBuilder Parent = p;
    public readonly string Name = n;
    public readonly uint Index = idx;
    
    public override string ToString() => $"Code block '{Name}'({Index})";
    public abstract string DumpInstructionsToString();
}
