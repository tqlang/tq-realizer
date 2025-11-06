namespace Tq.Realizer.Builder.ProgramMembers;

public interface INamespaceOrStructureOrTypedefBuilder
{
    public ModuleBuilder Module { get; }
    public string[] GlobalIdentifier { get; }

    public FunctionBuilder AddFunction(string symbol, bool isStatic);
}
