using Tq.Realizer.Builder.ProgramMembers;

namespace Tq.Realizer.Builder;

public class ProgramBuilder()
{

    private List<ModuleBuilder> _modules = [];
    public ModuleBuilder[] Modules => [.. _modules];
    
    internal void AddModule(ModuleBuilder module) => _modules.Add(module);
    public ModuleBuilder AddModule(string name)
    {
        var newmod = new ModuleBuilder(name);
        _modules.Add(newmod);
        return newmod;
    }

    public override string ToString() => string.Join("\n", _modules);

}
