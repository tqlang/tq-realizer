using Abstract.Realizer.Core.Intermediate.Values;

namespace Abstract.Realizer.Core.Intermediate.Language;

internal class IrSlice(SliceConstantValue v) : IrValue
{
    public SliceConstantValue Buffer = v;

    public override string ToString() => $"{Buffer}";
}
