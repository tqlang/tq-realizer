using System.ComponentModel;
using System.Diagnostics;
using Tq.Realizeer.Core.Program;
using Tq.Realizeer.Core.Program.Builder;
using Tq.Realizeer.Core.Program.Member;
using Tq.Realizer.Core.Builder.References;
using Tq.Realizer.Core.Configuration.LangOutput;

namespace Tq.Realizer.Passes;

internal static class Format
{

    public static void Pass(RealizerProgram program, IOutputConfiguration outConfig)
    {
        foreach (var i in program.Modules) UnnestProgramRecursive(i, outConfig);
    }

    private static void UnnestProgramRecursive(RealizerMember member, IOutputConfiguration outConfig)
    {
        var parent = member.Parent!;
        
        switch (member)
        {
            case RealizerModule module:
                foreach (var i in module.GetMembers().ToArray()) UnnestProgramRecursive(i, outConfig);
                break;
            
            case RealizerNamespace @nmsp:
            {
                foreach (var i in nmsp.GetMembers().ToArray()) UnnestProgramRecursive(i, outConfig);

                if ((outConfig.UnnestMembersOption & UnnestMembersOptions.NoNamespaces) == 0) return;
                
                parent.RemoveMember(nmsp);
                foreach (var i in nmsp.GetMembers().ToArray())
                {
                    nmsp.RemoveMember(i);
                    i.Name = nmsp.Name + '.' + i.Name;
                    parent.AddMember(i);
                }
                
            } break;

            case RealizerStructure @struct:
            {
                foreach (var i in @struct.GetMembers().ToArray()) UnnestProgramRecursive(i, outConfig);

                var nonmsp = (outConfig.UnnestMembersOption & UnnestMembersOptions.NoNamespaces) != 0;
                var forcestatic = (outConfig.UnnestMembersOption & UnnestMembersOptions.ForceStaticFunctions) != 0;
                
                if (!(nonmsp || forcestatic)) return;
                
                foreach (var i in @struct.GetMembers().ToArray())
                {
                    if (i is RealizerField { Static: false }) continue;
                    if (forcestatic && i is RealizerFunction { Static: false } @f) ConvertToStatic(f, @struct);

                    if (!nonmsp) continue;
                    @struct.RemoveMember(i);
                    i.Name = @struct.Name + '.' + i.Name;
                    parent.AddMember(i);
                }
                
            } break;
            
            case RealizerTypedef @typedef:
            {
                foreach (var i in typedef.GetMembers().ToArray()) UnnestProgramRecursive(i, outConfig);

                var nonmsp = (outConfig.UnnestMembersOption & UnnestMembersOptions.NoNamespaces) != 0;
                var forcestatic = (outConfig.UnnestMembersOption & UnnestMembersOptions.ForceStaticFunctions) != 0;
                
                if (!(nonmsp || forcestatic)) return;
                
                foreach (var i in typedef.GetMembers().ToArray())
                {
                    if (forcestatic && i is RealizerFunction { Static: false } @f) ConvertToStatic(f, typedef);

                    if (!nonmsp) continue;
                    typedef.RemoveMember(i);
                    i.Name = typedef.Name + '.' + i.Name;
                    parent.AddMember(i);
                }
                
            } break;
            
            
            case RealizerContainer: throw new UnreachableException();
            default: return;
        }
    }
    
    
    private static void ConvertToStatic(RealizerFunction instanceFunc, RealizerStructure structure)
    {
        instanceFunc.Static = true;
        instanceFunc.AddParameter("self", new ReferenceTypeReference(new NodeTypeReference(structure)), 0);
    }
    
    private static void ConvertToStatic(RealizerFunction instanceFunc, RealizerTypedef typedef)
    {
        instanceFunc.Static = true;
        instanceFunc.ReturnType = new NodeTypeReference(typedef);
        instanceFunc.AddParameter("value", new NodeTypeReference(typedef), 0);
    }
}
