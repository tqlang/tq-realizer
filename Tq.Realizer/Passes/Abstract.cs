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

internal static class Abstract
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

                if ((outConfig.AbstractingOptions & AbstractingOptions.UnwrapNamespaces) == 0) return;
                
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

                var nonmsp = (outConfig.AbstractingOptions & AbstractingOptions.UnwrapNamespaces) != 0;
                var forcestatic = (outConfig.AbstractingOptions & AbstractingOptions.NoSelfInstructions) != 0;
                
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

                var nonmsp = (outConfig.AbstractingOptions & AbstractingOptions.UnwrapNamespaces) != 0;
                var forcestatic = (outConfig.AbstractingOptions & AbstractingOptions.NoSelfInstructions) != 0;
                
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
            switch (i)
            {
                case OmegaCodeCell @omega:
                {
                    List<IOmegaInstruction> instructions = [];
                    foreach (var j in omega.Instructions)
                        instructions.Add(OmegaScanFunctionExecution(instanceFunc, j, outConfig));
                    omega.OverrideInstructions([..instructions]);
                } break;
            }
        }
    }


    private static IOmegaInstruction OmegaScanFunctionExecution(
        RealizerFunction instanceFunc,
        IOmegaInstruction i,
        IOutputConfiguration outConfig)
        => OmegaScanFunctionExecution(instanceFunc, i, outConfig, false, out _);
    
    private static IOmegaInstruction OmegaScanFunctionExecution(
        RealizerFunction instanceFunc,
        IOmegaInstruction i,
        IOutputConfiguration outConfig,
        
        // Those both works together: 
        bool reduceSelfAccess,      //  removes any "self" made by an access
        out bool doingSelfAccess)   //  returns true if it was an access that was using "self"
    {
        doingSelfAccess = false;
        
        while (true)
        {
            switch (i)
            {
                
                case Ret @r:
                    return new Ret(r.Value != null
                        ? (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, @r.Value, outConfig)
                        : null);

                case Assignment @a:
                {
                    if (a.Left is not Member { Node: RealizerProperty @p }) return new Assignment(
                        (IOmegaAssignable)OmegaScanFunctionExecution(instanceFunc, a.Left, outConfig),
                        (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Right, outConfig));
                    
                    var value = (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Right, outConfig);
                    var func = p.Setter!;

                    return p.Static
                        ? new Call(func.ReturnType, new Member(func), value)
                        : new Call(func.ReturnType, new Member(func), new Argument(instanceFunc.Parameters[0]), value);
                }
                
                case Call @c:
                {
                    var scannedArgs = c.Arguments.Select(e =>
                        (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, e, outConfig));
                    
                    var function = (IOmegaCallable)OmegaScanFunctionExecution(instanceFunc, c.Callable, outConfig,
                        true, out var wasFunctionDoingSelfAccess);
                    
                    return new Call(c.Type, function, wasFunctionDoingSelfAccess
                            ? [new Argument(instanceFunc.Parameters[0]), ..scannedArgs]
                            : [..scannedArgs]);
                }

                case Self:
                    return (outConfig.AbstractingOptions & AbstractingOptions.NoSelfInstructions) != 0
                        ? new Argument(instanceFunc.Parameters[0]) : i;

                case Access @a:
                {
                    if (a.Left is Self) doingSelfAccess = true;
                    
                    var accessL = (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Left, outConfig);
                    var accessR = (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Right, outConfig);
                    
                    return reduceSelfAccess && doingSelfAccess ? accessR : new Access(accessL, accessR);
                }

                case Member { Node: RealizerProperty @p } @m:
                {
                    var func = p.Getter!;
                    
                    if (p.Static)
                        return new Call(func.ReturnType, new Member(func), []);
                    
                    if ((outConfig.AbstractingOptions & AbstractingOptions.NoSelfInstructions) != 0)
                        return new Call(func.ReturnType, new Member(func), new Argument(instanceFunc.Parameters[0]));

                    return new Access(new Self(),
                        new Call(func.ReturnType, new Member(func), new Argument(instanceFunc.Parameters[0])));
                }

                case Add @a: return new Add(a.Type!,
                    (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Left, outConfig),
                    (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Right, outConfig));
                
                case Mul @a: return new Mul(a.Type!,
                    (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Left, outConfig),
                    (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Right, outConfig));
                
                case Cmp @a: return new Cmp(a.Op,
                    (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Left, outConfig),
                    (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Right, outConfig));
                
                case IntTypeCast @a: return new IntTypeCast((IntegerTypeReference)a.Type!,
                    (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Exp, outConfig));
                
                case IntFromPtr @a: return new IntFromPtr((IntegerTypeReference)a.Type!,
                    (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Exp, outConfig));
                
                case PtrFromInt @a: return new PtrFromInt((ReferenceTypeReference)a.Type!,
                    (IOmegaExpression)OmegaScanFunctionExecution(instanceFunc, a.Exp, outConfig));
                
                case Member:
                case Argument:
                case Constant:
                case Alloca:
                case Throw:
                    return i;
                
                default: throw new UnreachableException();
            }
        }
    }
}
