namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrRefOf(IrValue val) : IrValue
{
    public readonly IrValue Value = val;

    public override string ToString() => $"(refof {Value})";
}