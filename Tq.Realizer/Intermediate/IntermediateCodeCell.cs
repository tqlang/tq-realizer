using Tq.Realizeer.Core.Program.Builder;
using Tq.Realizer.Core.Builder.Execution;

namespace Tq.Realizer.Intermediate;

internal class IntermediateCodeCell(RealizerFunction s, string n, uint idx) : CodeCell(s, n, idx)
{
    //public readonly IrRoot Root = new IrRoot();
    public override bool IsFinished() => true;

    public override string DumpInstructionsToString() => ""; //Root.ToString();
}
