namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrAssign(IAssignable to, IrValue value) : IrNode
{
    public IAssignable to = to;
    public IrValue value = value;

    public override string ToString() => $"{to.ToString().TabNextLines()} = {value.ToString().TabNextLines()}";
}
