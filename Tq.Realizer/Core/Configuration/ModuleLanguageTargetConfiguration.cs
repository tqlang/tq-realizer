using Tq.Realizer.Builder;
using Tq.Realizer.Core.Configuration.LangOutput;

namespace Tq.Realizer.Core.Configuration;

public class ModuleLanguageTargetConfiguration
{
    public delegate void CompilerDelegate(ProgramBuilder program, ILanguageOutputConfiguration config);

    public string TargetName;
    public string TargetDescription;
    public string TargetIdentifier;

    public ILanguageOutputConfiguration LanguageOutput;
    public CompilerDelegate CompilerInvoke;
}
