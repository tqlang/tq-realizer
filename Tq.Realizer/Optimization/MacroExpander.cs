using Tq.Realizer.Builder.Language;
using Tq.Realizer.Builder.ProgramMembers;
using Tq.Realizer.Core.Configuration.LangOutput;
using Tq.Realizer.Core.Intermediate.Language;

namespace Tq.Realizer.Optimization;

public class MacroExpander
{
    internal static void ExpandFunctionMacros(FunctionBuilder function, ILanguageOutputConfiguration config)
    {
        foreach (var builder in function.CodeBlocks)
        {
            var intermediateRoot = ((IntermediateBlockBuilder)builder).Root;
            
            List<IrMacro> macros = [];
            foreach (var i in intermediateRoot.content) if (i is IrMacro @m) macros.Add(m);

            var localsIdx = 0;
        
            foreach (var macro in macros)
            {
                if (macro is IrMacroDefineLocal @macroDefineLocal)
                {
                    intermediateRoot.content.Remove(macroDefineLocal);
                    intermediateRoot.content.Insert(localsIdx++, @macroDefineLocal);
                }
            }
        }
    }
}