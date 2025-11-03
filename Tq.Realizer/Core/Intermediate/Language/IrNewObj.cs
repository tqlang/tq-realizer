using Tq.Realizer.Builder.ProgramMembers;

namespace Tq.Realizer.Core.Intermediate.Language;

internal class IrNewObj(StructureBuilder type) : IrValue
{
    public readonly StructureBuilder Type = type;
    public override string ToString() => $"(new {type.ToReadableReference()})";
}
