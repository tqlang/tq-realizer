using Abstract.Realizer.Core.Intermediate.Values;

namespace Abstract.Realizer.Core.Intermediate.Language;

internal class IrConstValue(RealizerConstantValue value) : IrValue
{
    public readonly RealizerConstantValue Value = value;
    public override string ToString() => Value.ToString()!;
}