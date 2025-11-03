namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrLocal(short index) : IrValue, IAssignable
{
    public readonly short Index = index;
    public override string ToString() => Index < 0 ? $"(arg {(-Index)-1})" : $"(local {Index})";
}
