using Tq.Realizeer.Core.Program;
using Tq.Realizeer.Core.Program.Member;
using Tq.Realizer.Core.Configuration.LangOutput;

namespace Tq.Realizer.Passes;

public class Abstract
{

    public static void Pass(RealizerProgram program, IOutputConfiguration outConfig)
    {
        foreach (var i in program.Modules) IterateRecursive(i, outConfig);
    }

    private static void IterateRecursive(RealizerMember member, IOutputConfiguration outConfig)
    {
        
    }
    
}