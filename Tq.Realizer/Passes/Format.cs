using System.ComponentModel;
using System.Diagnostics;
using Tq.Realizeer.Core.Program;
using Tq.Realizeer.Core.Program.Builder;
using Tq.Realizeer.Core.Program.Member;
using Tq.Realizer.Core.Builder.Execution.Omega;
using Tq.Realizer.Core.Builder.Language.Omega;
using Tq.Realizer.Core.Builder.References;
using Tq.Realizer.Core.Configuration.LangOutput;
using static Tq.Realizer.Core.Builder.Language.Omega.OmegaInstructions;

namespace Tq.Realizer.Passes;

internal static class Format
{

    public static void Pass(RealizerProgram program, IOutputConfiguration outConfig)
    {
        foreach (var i in program.Modules) FormatProgramRecursive(i, outConfig);
    }

    private static void FormatProgramRecursive(RealizerMember member, IOutputConfiguration outConfig)
    {
        var parent = member.Parent!;
        
        switch (member)
        {
            case RealizerModule module:
                foreach (var i in module.GetMembers().ToArray()) FormatProgramRecursive(i, outConfig);
                break;
            
            case RealizerNamespace @nmsp:
            {
                foreach (var i in nmsp.GetMembers().ToArray()) FormatProgramRecursive(i, outConfig);

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
                foreach (var i in @struct.GetMembers().ToArray()) FormatProgramRecursive(i, outConfig);

                var nonmsp = (outConfig.UnnestMembersOption & UnnestMembersOptions.NoNamespaces) != 0;
                var forcestatic = (outConfig.UnnestMembersOption & UnnestMembersOptions.ForceStaticFunctions) != 0;
                
                if (!(nonmsp || forcestatic)) return;
                
                foreach (var i in @struct.GetMembers().ToArray())
                {
                    if (i is RealizerField { Static: false }) continue;
                    if (forcestatic && i is RealizerFunction { Static: false } @f)
                    {
                        ConvertToStatic(f, @struct);
                        ScanFunctionExecution(f, outConfig);
                    }

                    if (!nonmsp) continue;
                    @struct.RemoveMember(i);
                    i.Name = @struct.Name + '.' + i.Name;
                    parent.AddMember(i);
                }
                
            } break;
            
            case RealizerTypedef @typedef:
            {
                foreach (var i in typedef.GetMembers().ToArray()) FormatProgramRecursive(i, outConfig);

                var nonmsp = (outConfig.UnnestMembersOption & UnnestMembersOptions.NoNamespaces) != 0;
                var forcestatic = (outConfig.UnnestMembersOption & UnnestMembersOptions.ForceStaticFunctions) != 0;
                
                if (!(nonmsp || forcestatic)) return;
                
                foreach (var i in typedef.GetMembers().ToArray())
                {
                    if (forcestatic && i is RealizerFunction { Static: false } @f)
                    {
                        ConvertToStatic(f, typedef);
                        ScanFunctionExecution(f, outConfig);
                    }

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


    private static void ScanFunctionExecution(RealizerFunction instanceFunc, IOutputConfiguration outConfig)
    {
        foreach (var i in instanceFunc.ExecutionBlocks)
        {
            if (i is OmegaCodeCell @omega)
            {
                List<IOmegaInstruction> instructions = [];
                foreach (var j in omega.Instructions)
                    instructions.Add(OmegaScanFunctionExecution(instanceFunc, j, outConfig));
                omega.OverrideInstructions([..instructions]);
            }
        }
    }
    private static IOmegaInstruction OmegaScanFunctionExecution(
        RealizerFunction instanceFunc,
        OmegaInstructions.IOmegaInstruction i,
        IOutputConfiguration outConfig)
    {
        while (true)
        {
            switch (i)
            {
                case Assignment @a:
                    return new Assignment(
                        (IOmegaAssignable)OmegaScanFunctionExecution(instanceFunc, a.Left, outConfig),
                        (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Right, outConfig));

                case Access @a:
                    return new Access(
                        (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Left, outConfig),
                        (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Right, outConfig));
                
                case Ret @r:
                    return new Ret(r.Value != null
                        ? (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, @r.Value, outConfig)
                        : null);
                
                case Call @c:
                    return new Call(
                        c.Type,
                        (IOmegaCallable)OmegaScanFunctionExecution(instanceFunc, c.Callable, outConfig),
                        c.Arguments.Select(e
                            => (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, e, outConfig)).ToArray());
                
                case Self:
                    return (outConfig.UnnestMembersOption & UnnestMembersOptions.ForceStaticFunctions) != 0
                        ? new Argument(instanceFunc.Parameters[0]) : i;
                
                case Member:
                case Argument:
                case Constant:
                case Throw:
                    return i;
                
                default: throw new UnreachableException();
            }
        }
    }
}
