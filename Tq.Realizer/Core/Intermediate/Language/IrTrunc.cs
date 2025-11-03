using Tq.Realizer.Core.Intermediate.Language;
using Tq.Realizer.Core.Intermediate.Types;

namespace Tq.Realizer.Builder.ProgramMembers;

internal class IrTrunc(IntegerType to, IrValue val) : IrValue
{
    public readonly IntegerType ToType = to;
    public readonly IrValue Value = val;

    public override string ToString() => $"(trunc {ToType} {Value})";
}
