using Tq.Realizer.Core.Intermediate.Values;

namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrConstValue(RealizerConstantValue value) : IrValue
{
    public readonly RealizerConstantValue Value = value;
    public override string ToString() => Value.ToString()!;
}