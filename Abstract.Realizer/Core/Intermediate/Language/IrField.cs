using Abstract.Realizer.Builder.ProgramMembers;

namespace Abstract.Realizer.Core.Intermediate.Language;

internal class IrField(FieldBuilder f): IrValue, IAssignable
{
    public readonly FieldBuilder Field = f;

    public override string ToString() => $"(field {Field.ToReadableReference()})";
}