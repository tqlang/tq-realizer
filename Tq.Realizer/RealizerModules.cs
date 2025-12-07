using System.Reflection;
using Tq.Realizer.Core.Configuration;
using Tq.Realizer.Core.Configuration.LangOutput;

namespace Tq.Realizer;

public static class RealizerModules
{

    private static string ModulesPath => string.Concat(AppContext.BaseDirectory, "Modules");
    private static List<IModule> _modules = [];
    public static IModule[] Modules => [.. _modules];
    public static TargetConfiguration[] Targets => _modules
        .Select(e => e.Config)
        .SelectMany(e => e.Targets).ToArray();
    
    public static void AutoLoad()
    {
        if (!Directory.Exists(ModulesPath)) return;
        var files = Directory.EnumerateFiles(ModulesPath, "Tq.Module.*.dll", SearchOption.AllDirectories);
        foreach (var file in files) LoadSingle(file);
    }
    public static void ManualLoad(string modulePath) => LoadSingle(modulePath);

    private static void LoadSingle(string modulePath)
    {
        var asm = Assembly.LoadFile(modulePath);
        var modules = asm.GetTypes().Where(e => e.IsAssignableTo(typeof(IModule)));

        foreach (var module in modules)
        {
            var minstance = (IModule)Activator.CreateInstance(module)!;
            _modules.Add(minstance);
            Console.WriteLine($"Loaded module {minstance.Config.Name} v.{minstance.Config.Version}");
        }
    }
    
    public static TargetConfiguration Find(string identifier) => Targets
        .FirstOrDefault(e => e.TargetIdentifier == identifier)!;
}