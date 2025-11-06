using Tq.Realizer.Builder.References;
using Tq.Realizer.Core.Intermediate.Values;

namespace Tq.Realizer.Builder.ProgramMembers;

public abstract class FieldBuilder: ProgramMemberBuilder
{
    public TypeReference? Type = null;
    public RealizerConstantValue? Value = null;
    
    internal FieldBuilder(INamespaceOrStructureBuilder parent, string name)
        : base(parent, name) {}
}
