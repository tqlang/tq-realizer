using Tq.Realizer.Builder.ProgramMembers;
using Tq.Realizer.Core.Intermediate.Language;

namespace Tq.Realizer.Builder.Language;

internal class IntermediateBlockBuilder(FunctionBuilder p, string n, uint idx) : BlockBuilder(p, n, idx)
{
    public readonly IrRoot Root = new IrRoot();
    public override string DumpInstructionsToString() => Root.ToString();
}
