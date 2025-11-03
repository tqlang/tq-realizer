namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrAccess(IrValue left, IrValue right) : IrValue, IAssignable
{
    public IrValue Left = left;
    public IrValue Right = right;

    public override string ToString() => $"{Left.ToString().TabNextLines()}->{Right.ToString().TabNextLines()}";
}