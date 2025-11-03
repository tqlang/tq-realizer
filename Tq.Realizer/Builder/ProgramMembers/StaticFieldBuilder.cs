using Tq.Realizer.Builder.References;
using Tq.Realizer.Core.Intermediate.Values;

namespace Tq.Realizer.Builder.ProgramMembers;

public class StaticFieldBuilder: FieldBuilder
{
    public TypeReference? Type = null;
    public RealizerConstantValue? Value = null;
    
    internal StaticFieldBuilder(INamespaceOrStructureBuilder parent, string name, bool annonymous)
        : base(parent, name, annonymous) {}
    
    
    public override string ToString() => $"(field static \"{Symbol}\" {Type?.ToString() ?? "<nil>"}"
                                        + (Value != null ? $" {Value}" : "") + ")";
    public override string ToReadableReference() => $"\"{string.Join('.', GlobalIdentifier)}\"";
}
