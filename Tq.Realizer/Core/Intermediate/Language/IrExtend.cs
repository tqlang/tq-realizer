using Tq.Realizer.Builder.References;
using Tq.Realizer.Core.Intermediate.Language;
using Tq.Realizer.Core.Intermediate.Types;

namespace Tq.Realizer.Builder.ProgramMembers;

internal class IrExtend(IntegerType to, IrValue val) : IrValue
{
    public readonly IntegerType ToType = to;
    public readonly IrValue Value = val;

    public override string ToString() => $"(extend {ToType} {Value})";
}
