using Tq.Realizer.Core.Intermediate.Types;

namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrBinaryOp(RealizerType type, BinaryOperation op, IrValue left, IrValue right) : IrValue
{
    public readonly RealizerType Type = type;
    public readonly BinaryOperation Op = op;
    public readonly IrValue Left = left;
    public readonly IrValue Right = right;

    public override string ToString() => $"({Type}.{op.ToString().Replace('_', '.')}"
                                         + $"\n{Left.ToString().TabAllLines()}"
                                         + $"\n{Right.ToString().TabAllLines()})";
}
