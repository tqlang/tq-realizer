using Tq.Realizeer.Core.Program;
using Tq.Realizeer.Core.Program.Builder;
using Tq.Realizeer.Core.Program.Member;
using Tq.Realizer.Core.Configuration.LangOutput;

namespace Tq.Realizer.Passes;

public class Analysis
{
    
    public static void Pass(RealizerProgram program, IOutputConfiguration outConfig)
    {
        foreach (var i in program.Modules) IterateRecursive(i, outConfig);
    }

    private static void IterateRecursive(RealizerMember member, IOutputConfiguration outConfig)
    {
        switch (member)
        {
            case RealizerStructure s: PackStructure(s, outConfig); break;
        }

        if (member is not RealizerContainer container) return;
        foreach (var i in container.GetMembers())
            IterateRecursive(i, outConfig);
    }

    private static void PackStructure(RealizerStructure structure, IOutputConfiguration outConfig)
    {
        List<RealizerField> fields = [];
        foreach (var i in structure.GetMembers<RealizerField>().ToArray())
        {
            structure.RemoveMember(i);
            fields.Add(i);
            i.OverrideBitAlignment(i.Type!.Alignment);
        }
        
        fields.Sort((a, b) =>
            a.BitAlignment.ToInt(outConfig.NativeIntegerSize) 
            - b.BitAlignment.ToInt(outConfig.NativeIntegerSize));
        
        var off = 0;
        for (int i = 0; i < fields.Count; i++)
        {
            var f = fields[i];
            
            structure.AddMember(f, i);
            f.OverrideIndex((uint)i);
            f.OverrideBitOffset(off);
            
            off += f.Type!.Length.ToInt(outConfig.NativeIntegerSize);
        }
        
    }
}
