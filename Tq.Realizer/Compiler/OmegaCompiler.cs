using Tq.Realizer.Core.Builder.Execution.Omega;
using Tq.Realizer.Core.Configuration.LangOutput;
using Tq.Realizer.Intermediate;

namespace Tq.Realizer.Compiler;

internal static class OmegaCompiler
{
    public static OmegaCodeCell CompileBlock(
        IntermediateCodeCell intermediateCodeCell,
        OmegaOutputConfiguration config)
    {
        var block = new OmegaCodeCell(intermediateCodeCell.Source, intermediateCodeCell.Name, intermediateCodeCell.Index);
        //UnwrapNode(block, intermediateBlock.Root, DataMode.Load, config);
        return block;
    }
    

    private enum DataMode
    {
        Load,
        LoadRef,
        
        Store, 
        StoreRef
    }
}
