using Tq.Realizer.Builder.ProgramMembers;

namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrField(FieldBuilder f): IrValue, IAssignable
{
    public readonly FieldBuilder Field = f;

    public override string ToString() => $"(field {Field.ToReadableReference()})";
}