namespace Tq.Realizer.Builder.ProgramMembers;

public abstract class FieldBuilder: ProgramMemberBuilder
{
    internal FieldBuilder(INamespaceOrStructureBuilder parent, string name, bool annonymous)
        : base(parent, name, annonymous) {}
}
