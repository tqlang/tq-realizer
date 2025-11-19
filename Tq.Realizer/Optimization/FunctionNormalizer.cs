using System.Diagnostics;
using Tq.Realizer.Builder.Language;
using Tq.Realizer.Builder.ProgramMembers;
using Tq.Realizer.Core.Configuration.LangOutput;
using Tq.Realizer.Core.Intermediate.Language;

namespace Tq.Realizer.Optimization;

internal static class FunctionNormalizer
{
    internal static void NormalizeFunction(FunctionBuilder function, ILanguageOutputConfiguration config)
    {

        var ctx = new FunctionCtx
        {
            NeedsMemStack = false, //NeedsMemStack(function, config),
            NeedsToConvertLdSelfToLdArg = NeedsToConvertLdSelfToLdArg(function, config)
        };
        
        foreach (var builder in function.CodeBlocks)
        {
            var intermediateRoot = ((IntermediateBlockBuilder)builder).Root;
            ScanNodesRecursive(intermediateRoot, ctx);
        }
        
    }

    private static IrNode ScanNodesRecursive(IrNode node, FunctionCtx ctx)
    {
        switch (node)
        {
            case IrRoot @root:
                for (var i = 0; i < root.content.Count; i++)
                    root.content[i] = ScanNodesRecursive(root.content[i], ctx);
                return root;
            
            case IrRet @ret:
                if (ret.Value != null) ret.Value = (IrValue)ScanNodesRecursive(ret.Value, ctx);
                return ret;
            
            case IrAssign @assign:
                 assign.to = (IAssignable)ScanNodesRecursive((IrNode)assign.to, ctx);
                 assign.value = (IrValue)ScanNodesRecursive(assign.value, ctx);
                return assign;
            
            case IrAccess @access:
                access.Left = (IrValue)ScanNodesRecursive(access.Left, ctx);
                access.Right = (IrValue)ScanNodesRecursive(@access.Right, ctx);
                return access;
            
            case IrSelf:
                return ctx.NeedsToConvertLdSelfToLdArg
                    ? new IrLocal(-1)
                    : node;
                
            case IrLocal @local:
                return local.Index < 0
                    ? new IrLocal((short)(local.Index - 1))
                    : node;
            
            case IrField @field:
                return node;

            default: throw new UnreachableException();
        }
        
    }
    

    private static bool NeedsMemStack(FunctionBuilder function, BetaOutputConfiguration config)
    {
        return config.UseMemoryStack
               || config.LocalStores == BetaDataKinds.All && false;
    }

    private static bool NeedsToConvertLdSelfToLdArg(FunctionBuilder function, ILanguageOutputConfiguration config)
    {
        if (function.IsStatic) return false;
        if ((config.GenericAllowedFeatures & GenericAllowedFeatures.UseLdSelfInsteadArg0) != 0) return true;
        return false;
    }
    
    
    private struct FunctionCtx
    {
        public bool NeedsMemStack;
        public bool NeedsToConvertLdSelfToLdArg;
    }
}
