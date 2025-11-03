using Tq.Realizer.Core.Intermediate.Types;

namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrConv(RealizerType t, IrValue v) : IrValue
{
    public readonly IrValue Value = v;
    public readonly RealizerType Type = t;

    public override string ToString() => $"(conv {Type} {Value})";
}