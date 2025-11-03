using System.Text;
using Tq.Realizer.Builder.Language;

namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrBranch(BlockBuilder to) : IrNode
{
    public readonly BlockBuilder To = to;

    public override string ToString() => $"(branch $\"{To.Name}\")";

}
