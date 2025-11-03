using Tq.Realizer.Core.Intermediate.Values;

namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrSlice(SliceConstantValue v) : IrValue
{
    public SliceConstantValue Buffer = v;

    public override string ToString() => $"{Buffer}";
}
