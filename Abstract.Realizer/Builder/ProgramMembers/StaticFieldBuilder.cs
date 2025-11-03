using Abstract.Realizer.Builder.References;
using Abstract.Realizer.Core.Intermediate.Values;

namespace Abstract.Realizer.Builder.ProgramMembers;

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
