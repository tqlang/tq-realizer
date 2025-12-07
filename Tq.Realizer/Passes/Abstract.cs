using System.Diagnostics;
using Tq.Realizeer.Core.Program;
using Tq.Realizeer.Core.Program.Builder;
using Tq.Realizeer.Core.Program.Member;
using Tq.Realizer.Core.Builder.Execution.Omega;
using Tq.Realizer.Core.Builder.References;
using Tq.Realizer.Core.Configuration.LangOutput;
using static Tq.Realizer.Core.Builder.Language.Omega.OmegaInstructions;

namespace Tq.Realizer.Passes;

internal class Abstract: IProcessingPass
{
    private RealizerProgram _program;
    private IOutputConfiguration _outConfig;
    
    private bool unwrapContainers => (_outConfig.AbstractingOptions & AbstractingOptions.NoNamespaces) != 0;
    private bool noInstanceMethod => (_outConfig.AbstractingOptions & AbstractingOptions.NoInstanceMethod) != 0;
    private bool noInheritance => (_outConfig.AbstractingOptions & AbstractingOptions.NoInheritance) != 0;
    private bool canUseLdSelf => (_outConfig.GenericAllowedFeatures & GenericAllowedFeatures.LdSelf) != 0;
    
    public void Pass(RealizerProgram program, IOutputConfiguration outConfig)
    {
        _program = program;
        _outConfig = outConfig;
        
        foreach (var module in program.Modules)
            FormatProgramRecursive(module);
    }

    private void FormatProgramRecursive(RealizerMember member)
    {
        var parent = member.Parent!;

        switch (member)
        {
            case RealizerModule module:
                foreach (var m in module.GetMembers().ToArray()) FormatProgramRecursive(m);
                break;

            case RealizerNamespace nmsp:
            {
                foreach (var m in nmsp.GetMembers().ToArray()) FormatProgramRecursive(m);
                
                if (!unwrapContainers) return;
                parent.RemoveMember(nmsp);
                foreach (var m in nmsp.GetMembers().ToArray())
                {
                    nmsp.RemoveMember(m);
                    m.Name = nmsp.Name + "." + m.Name;
                    parent.AddMember(m);
                }
                
            } break;

            case RealizerStructure @struct:
            case RealizerTypedef typedef:
            {
                var container = (RealizerContainer)member;

                foreach (var m in container.GetMembers().ToArray())
                    FormatProgramRecursive(m);
                
                if (!unwrapContainers) return;
                foreach (var m in container.GetMembers().ToArray())
                {
                    if (!unwrapContainers) continue;
                    switch (m)
                    {
                        case RealizerField { Static: false }: continue;
                        case RealizerProperty prop: container.RemoveMember(prop); continue;
                    }

                    container.RemoveMember(m);
                    m.Name = member.Name + "." + m.Name;
                    parent.AddMember(m);
                }
            } break;

            case RealizerContainer:
                throw new UnreachableException();

            case RealizerFunction fun:
            {
                if (!fun.Static && (noInstanceMethod || !canUseLdSelf))
                {
                    switch (parent)
                    {
                        case RealizerStructure structure: ConvertToStatic(fun, structure); break;
                        case RealizerTypedef typedef: ConvertToStatic(fun, typedef); break;
                    }
                }
                // Fallback to generic scan
                else ScanFunctionExecution(fun);
            } break;

            default: return;
        }
    }
    
    private void ConvertToStatic(RealizerFunction instanceFunc, RealizerStructure structure)
    {
        instanceFunc.Static = true;
        var self = instanceFunc.AddParameter("__self__",
            new ReferenceTypeReference(new NodeTypeReference(structure)), 0);
        OmegaScanFunctionExecution(instanceFunc, new Argument(self));
    }
    private void ConvertToStatic(RealizerFunction instanceFunc, RealizerTypedef typedef)
    {
        instanceFunc.Static = true;
        var self = instanceFunc.AddParameter("__self__",
            new ReferenceTypeReference(new NodeTypeReference(typedef)), 0);
        OmegaScanFunctionExecution(instanceFunc, new Argument(self));
    }
 
    
    private void ScanFunctionExecution(RealizerFunction func)
    {
        OmegaScanFunctionExecution(
            func,
            func.Static ? null : canUseLdSelf ? new Self() : throw new UnreachableException()
        );
    }

    private void OmegaScanFunctionExecution(
        RealizerFunction func,
        IOmegaExpression? selfReference
    )
    {

        foreach (var block in func.ExecutionBlocks)
        {
            switch (block)
            {
                case OmegaCodeCell @o:
                {
                    List<IOmegaInstruction> instructions = [];
                    foreach (var i in o.Instructions)
                    {
                        instructions.Add(
                            OmegaScanInstruction(
                                func,
                                selfReference,
                                i,
                                AccessMode.Load
                            ).res);
                    }
                    o.OverrideInstructions([.. instructions]);
                } break;
                
                default: throw new NotImplementedException();
            }
        }
        
    }

    private (IOmegaInstruction res, IOmegaExpression? subaccess) OmegaScanInstruction(
        RealizerFunction func,
        IOmegaExpression? selfReference,
        IOmegaInstruction instruction,
        AccessMode accessMode
    )
    {
        switch (instruction)
        {
            case Ret @r:
            {
                var v = r.Value != null
                    ? (IOmegaExpression)OmegaScanInstruction(func, selfReference, @r.Value, AccessMode.Load).res
                    : null;
                return (new Ret(v), null);
            }

            case Call @c:
            {
                var (fun, instance) = OmegaScanInstruction(func, selfReference, c.Callable, AccessMode.Load);
                List<IOmegaExpression> arguments = [];

                foreach (var i in c.Arguments)
                    arguments.Add((IOmegaExpression)OmegaScanInstruction(func, selfReference, i, AccessMode.Load).res);
                
                IOmegaCallable callable = (IOmegaCallable)fun;
                IOmegaExpression[] args = callable.Type.IsStatic
                    ? [..arguments]
                    : [selfReference!, ..arguments];
                
                return (new Call(callable, args), null);
            }

            case Assignment @a:
            {
                var (assignable, inst) = OmegaScanInstruction(func, selfReference, a.Left, AccessMode.Load);
                var value = (IOmegaExpression)OmegaScanInstruction(func, selfReference, a.Right, AccessMode.Load).res;

                if (assignable is Member { Node: RealizerProperty @p })
                {
                    var setter = p.Setter!;
                    return (new Call(new Member(setter), inst!), null);
                }
                return (new Assignment((IOmegaAssignable)assignable, value), null);
            }

            case Access @a:
            {
                var (accessLeftA, accessLeftB) = OmegaScanInstruction(func, selfReference, @a.Left, AccessMode.Load);
                var (accessRightA, accessRightB) = OmegaScanInstruction(func, selfReference, @a.Right, AccessMode.Load);

                if (accessLeftB != null || accessRightB != null) throw new UnreachableException();
                
                if (accessRightA is Member @member)
                {
                    switch (member.Node)
                    {
                        case RealizerField: goto fallback;
                        
                        case RealizerFunction:
                            return (accessRightA, (IOmegaExpression)accessLeftA);

                        case RealizerProperty @p:
                        {
                            if (accessMode == AccessMode.Load)
                            {
                                var getter = p.Getter!;
                                return (new Call( new Member(getter), (IOmegaExpression)accessLeftA), null);
                            }
                            return (new Member(p), (IOmegaExpression)accessLeftA);
                        }
                        
                        default: throw new NotImplementedException();
                    }
                }
                
                fallback:
                return (new Access((IOmegaExpression)accessLeftA, (Member)accessRightA), null);
            }


            case Add @a:
            {
                var left = (IOmegaExpression)OmegaScanInstruction(func, selfReference, a.Left, AccessMode.Load).res;
                var right = (IOmegaExpression)OmegaScanInstruction(func, selfReference, a.Right, AccessMode.Load).res;
                return (new Add(a.Type!, left, right), null);
            }
            case Mul @a:
            {
                var left = (IOmegaExpression)OmegaScanInstruction(func, selfReference, a.Left, AccessMode.Load).res;
                var right = (IOmegaExpression)OmegaScanInstruction(func, selfReference, a.Right, AccessMode.Load).res;
                return (new Mul(a.Type!, left, right), null);
            }

            case Cmp @c:
            {
                var left = (IOmegaExpression)OmegaScanInstruction(func, selfReference, c.Left, AccessMode.Load).res;
                var right = (IOmegaExpression)OmegaScanInstruction(func, selfReference, c.Right, AccessMode.Load).res;
                return (new Cmp(c.Op, left, right), null);
            }
            
            case Self: return (selfReference!, null);
            
            case Val v: return (new Val((IOmegaExpression)OmegaScanInstruction(func, selfReference, v.Expression, AccessMode.Load).res), null);
            case Ref r: return (new Ref((IOmegaExpression)OmegaScanInstruction(func, selfReference, r.Expression, AccessMode.Load).res), null);
            case IntFromPtr r: return (new Ref((IOmegaExpression)OmegaScanInstruction(func, selfReference, r.Expression, AccessMode.Load).res), null);
            case PtrFromInt r: return (new Ref((IOmegaExpression)OmegaScanInstruction(func, selfReference, r.Expression, AccessMode.Load).res), null);
            case Typeof t: return (new Typeof((IOmegaExpression)OmegaScanInstruction(func, selfReference, t.Expression, AccessMode.Load).res), null);
            
            case Member:
            case Alloca:
            case Argument:
            case Constant:
            case Register:
                return (instruction, null);
            
            default: throw new NotImplementedException();
        }
    }
    
    private enum AccessMode { Load, Store }
}
