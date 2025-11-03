namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrTypeOf(IrValue val) : IrValue
{
    public readonly IrValue Value = val;
    public override string ToString() => $"(typeof {Value})";
}