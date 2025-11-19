using System.Diagnostics;
using Tq.Realizer.Builder.ProgramMembers;

namespace Tq.Realizer.Builder.References;

public class NodeTypeReference : TypeReference
{
    public readonly ProgramMemberBuilder TypeReference;
    public sealed override uint? Alignment
    {
        get => TypeReference switch
            {
                StructureBuilder @struct => @struct.Alignment,
                _ => throw new UnreachableException(),
            };
        init { }
    }

    public NodeTypeReference(TypedefBuilder typedef) => TypeReference = typedef;

    public NodeTypeReference(StructureBuilder structure) =>TypeReference = structure;

    public override string ToString() => TypeReference.ToReadableReference();
}