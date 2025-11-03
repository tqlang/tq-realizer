namespace Tq.Realizer.Builder.ProgramMembers;

public interface INamespaceOrStructureBuilder
{
    public ModuleBuilder Module { get; }
    public string[] GlobalIdentifier { get; }
}