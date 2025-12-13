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
    private bool initFieldsOnCall => (_outConfig.GenericAllowedFeatures & GenericAllowedFeatures.InitializeFieldsOnCall) != 0;
    
    
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

        foreach (var block in func.ExecutionBlocks.ToArray())
        {
            switch (block)
            {
                case OmegaCodeCell @o:
                {
                    var newCodeCell = func.ReplaceOmegaCodeCell(o);
                    foreach (var i in o.Instructions)
                        OmegaScanInstruction(func, newCodeCell, selfReference, i);
                } break;
                
                default: throw new NotImplementedException();
            }
        }
        
    }

    private void OmegaScanInstruction(
        RealizerFunction func,
        OmegaCodeCell cell,
        IOmegaExpression? selfReference,
        IOmegaInstruction instruction)
    {
        switch (instruction)
        {
            case Ret @r:
            {
                var v = r.Value == null ? null
                    : OmegaScanExpression(func, cell, selfReference, r.Value, AccessMode.Load)._ref;
                cell.Writer.Ret(v);
            } break;
            
            case Assignment @a:
            {
                var (assignable, inst) = OmegaScanExpression(func, cell, selfReference, a.Left, AccessMode.Load);
                var value = OmegaScanExpression(func, cell, selfReference, a.Right, AccessMode.Load)._ref;

                if (assignable is Member { Node: RealizerProperty @p })
                {
                    var setter = p.Setter!;
                    cell.Writer.Call(new Member(setter), inst!);
                }
                cell.Writer.Assignment((IOmegaAssignable)assignable, value);
            } break;

            case Call @c:
            {
                var (callableRef, callableBase) = OmegaScanExpression(
                    func, cell, selfReference, c.Callable, AccessMode.Load);
                
                IOmegaExpression? arg0 = null;
                if (callableBase != null)
                {
                    if (callableBase is not Argument)
                    {
                        arg0 = cell.Writer.GetNewRegister(callableBase.Type!);
                        cell.Writer.Assignment((Register)arg0, callableBase);
                    }
                    else arg0 = callableBase;
                }

                List<IOmegaExpression> args = [];
                if (arg0 != null) args.Add(arg0);
                args.AddRange(c.Arguments.Select(
                    arg=> OmegaScanExpression(func, cell, selfReference, arg, AccessMode.Load)._ref));

                cell.Writer.Call((IOmegaCallable)callableRef, [..args]);
            } break;
            
            case CallIntrinsic @ci:
            {
                switch (ci.Function)
                {
                    case IntrinsicFunctions.initFields: break;
                    default: throw new NotImplementedException();
                }
            } break;
            
            default: throw new NotImplementedException();
        }
    }

    private (IOmegaExpression _ref, IOmegaExpression? _base) OmegaScanExpression(
        RealizerFunction func,
        OmegaCodeCell cell,
        IOmegaExpression? selfReference,
        IOmegaExpression instruction,
        AccessMode accessMode)
    {
        switch (instruction)
        {
            case Call @c:
            {
                var (callableRef, callableBase) = OmegaScanExpression(
                    func, cell, selfReference, c.Callable, AccessMode.Load);
                
                IOmegaExpression? arg0 = null;
                if (callableBase != null)
                {
                    if (callableBase is not Argument)
                    {
                        arg0 = cell.Writer.GetNewRegister(callableBase.Type!);
                        cell.Writer.Assignment((Register)arg0, callableBase);
                    }
                    else arg0 = callableBase;
                }

                List<IOmegaExpression> args = [];
                if (arg0 != null) args.Add(arg0);
                args.AddRange(c.Arguments.Select(
                    arg=> OmegaScanExpression(func, cell, selfReference, arg, AccessMode.Load)._ref));

                return (new Call((IOmegaCallable)callableRef, [..args]), null);
            }

            case CallIntrinsic @ci:
            {
                switch (ci.Function)
                {
                    default:
                        throw new NotImplementedException();
                }
            }

            case Access @a:
            {
                var (accessLeftA, accessLeftB) = OmegaScanExpression(func, cell, selfReference, @a.Left, AccessMode.Load);
                var (accessRightA, accessRightB) = OmegaScanExpression(func, cell, selfReference, @a.Right, AccessMode.Load);

                if (accessLeftB != null || accessRightB != null) throw new UnreachableException();
                
                if (accessRightA is Member @member)
                {
                    switch (member.Node)
                    {
                        case RealizerField: goto fallback;
                        
                        case RealizerFunction:
                            return (accessRightA, accessLeftA);

                        case RealizerProperty @p:
                        {
                            if (accessMode == AccessMode.Load)
                            {
                                var getter = p.Getter!;
                                return (new Call( new Member(getter), accessLeftA), null);
                            }
                            return (new Member(p), accessLeftA);
                        }
                        
                        default: throw new NotImplementedException();
                    }
                }
                
                fallback:
                return (new Access(accessLeftA, (Member)accessRightA), null);
            }


            case Add @a:
            {
                var left = OmegaScanExpression(func, cell, selfReference, a.Left, AccessMode.Load)._ref;
                var right = OmegaScanExpression(func, cell, selfReference, a.Right, AccessMode.Load)._ref;
                return (new Add(a.Type!, left, right), null);
            }
            case Mul @a:
            {
                var left = OmegaScanExpression(func, cell, selfReference, a.Left, AccessMode.Load)._ref;
                var right = OmegaScanExpression(func, cell, selfReference, a.Right, AccessMode.Load)._ref;
                return (new Mul(a.Type!, left, right), null);
            }

            case Cmp @c:
            {
                var left = OmegaScanExpression(func, cell, selfReference, c.Left, AccessMode.Load)._ref;
                var right = OmegaScanExpression(func, cell, selfReference, c.Right, AccessMode.Load)._ref;
                return (new Cmp(c.Op, left, right), null);
            }
            
            case Self: return (selfReference!, null);
            
            case Val v:return (new Val(OmegaScanExpression(func, cell, selfReference, v.Expression, AccessMode.Load)._ref), null);
            case Ref r: return (new Ref(OmegaScanExpression(func, cell, selfReference, r.Expression, AccessMode.Load)._ref), null);
            case IntFromPtr r: return (new Ref(OmegaScanExpression(func, cell, selfReference, r.Expression, AccessMode.Load)._ref), null);
            case PtrFromInt r: return (new Ref(OmegaScanExpression(func, cell, selfReference, r.Expression, AccessMode.Load)._ref), null);
            case Typeof t: return (new Typeof(OmegaScanExpression(func, cell, selfReference, t.Expression, AccessMode.Load)._ref), null);
            
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
