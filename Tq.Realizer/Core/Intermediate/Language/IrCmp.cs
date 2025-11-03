namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrCmp(bool signed, CompareOperation op, IrValue left, IrValue right) : IrValue
{
    public readonly bool Signed = signed;
    public readonly CompareOperation Op = op;
    public readonly IrValue Left = left;
    public readonly IrValue Right = right;

    public override string ToString() => "(" + (Signed ? 's' : 'u') + $".{op.ToString().Replace('_', '.')}"
                                         + $"\n{Left.ToString().TabAllLines()}"
                                         + $"\n{Right.ToString().TabAllLines()})";
}