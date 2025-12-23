using Tq.Realizer.Core.Program;
using Tq.Realizer.Core.Configuration.LangOutput;

namespace Tq.Realizer.Passes;

public interface IProcessingPass
{
    public void Pass(RealizerProgram program, IOutputConfiguration outConfig);
}
