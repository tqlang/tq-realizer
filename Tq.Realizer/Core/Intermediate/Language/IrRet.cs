using Tq.Realizer.Core.Intermediate.Language;

namespace Tq.Realizer.Builder.ProgramMembers;

internal class IrRet(IrValue? val): IrNode
{
    public readonly IrValue? Value = val;

    public override string ToString() => Value == null
        ? "ret"
        : $"retv {Value}";
}
