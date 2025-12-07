using Tq.Realizeer.Core.Program;
using Tq.Realizeer.Core.Program.Builder;
using Tq.Realizeer.Core.Program.Member;
using Tq.Realizer.Core.Configuration.LangOutput;

namespace Tq.Realizer.Passes;

public class Analysis: IProcessingPass
{

    private RealizerProgram _program;
    private IOutputConfiguration _outConfig;
    
    public void Pass(RealizerProgram program, IOutputConfiguration outConfig)
    {
        _program = program;
        _outConfig = outConfig;
        
        foreach (var i in program.Modules) IterateRecursive(i);
    }

    private void IterateRecursive(RealizerMember member)
    {
        switch (member)
        {
            case RealizerStructure s: PackStructure(s); break;
        }

        if (member is not RealizerContainer container) return;
        foreach (var i in container.GetMembers())
            IterateRecursive(i);
    }

    private void PackStructure(RealizerStructure structure)
    {
        List<RealizerField> fields = [];
        foreach (var i in structure.GetMembers<RealizerField>().ToArray())
        {
            structure.RemoveMember(i);
            fields.Add(i);
            i.OverrideBitAlignment(i.Type!.Alignment);
        }
        
        fields.Sort((a, b) =>
            a.BitAlignment.ToInt(_outConfig.NativeIntegerSize) 
            - b.BitAlignment.ToInt(_outConfig.NativeIntegerSize));
        
        var off = 0;
        for (int i = 0; i < fields.Count; i++)
        {
            var f = fields[i];
            
            structure.AddMember(f, i);
            f.OverrideIndex((uint)i);
            f.OverrideBitOffset(off);
            
            off += f.Type!.Length.ToInt(_outConfig.NativeIntegerSize);
        }
        
    }
}
