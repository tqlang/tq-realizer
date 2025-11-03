using System.Diagnostics;
using Tq.Realizer.Builder;
using Tq.Realizer.Builder.ProgramMembers;

namespace Tq.Realizer.Optimization;

internal static class Unnester
{
    internal static void UnnestProgram(ProgramBuilder builder)
    {
        Dictionary<ModuleBuilder, ModuleContent> moduleMembers = [];
        
        foreach (var module in builder.Modules)
        {
            var mc = new ModuleContent();
            SelectMembersRecursive(module, module, mc);
            moduleMembers.Add(module, mc);
        }

        foreach (var (module, content) in moduleMembers)
        {
            module.fields.Clear();
            module.functions.Clear();
            module.structures.Clear();
            module.typedefs.Clear();
            module.namespaces.Clear();
            
            module.fields = content.fields;
            module.functions = content.functions;
            module.structures = content.structs;
            module.typedefs = content.typedefs;
            
        }
    }

    private static void SelectMembersRecursive(ProgramMemberBuilder member, ModuleBuilder module, ModuleContent content)
    {
        switch (member)
        {
            case NamespaceBuilder @nmsp:
            {
                foreach (var nmsps in nmsp.Namespaces) SelectMembersRecursive(nmsps, module, content);
                foreach (var fields in nmsp.Fields) SelectMembersRecursive(fields, module, content);
                foreach (var strucs in nmsp.Structures) SelectMembersRecursive(strucs, module, content);
                foreach (var funcs in nmsp.Functions) SelectMembersRecursive(funcs, module, content);
                foreach (var typedef in nmsp.TypeDefinitions) SelectMembersRecursive(typedef, module, content);
                nmsp.namespaces.Clear();
                nmsp.fields.Clear();
                nmsp.structures.Clear();
                nmsp.functions.Clear();
                nmsp.typedefs.Clear();
            } break;

            case StructureBuilder @struc:
                content.structs.Add(struc);
                
                foreach (var sfields in struc.StaticFields) SelectMembersRecursive(sfields, module, content);
                struc.StaticFields.Clear();
                break;
            
            case TypeDefinitionBuilder @typedef:
                content.typedefs.Add(typedef);
                //TODO Typedef functions
                break;
            
            case BaseFunctionBuilder @func: content.functions.Add(func); break;
            case StaticFieldBuilder @field: content.fields.Add(field); break;
            
            default: throw new UnreachableException();
        }

        if (member == module) return;
        
        var global = string.Join('.', member.Parent?.GlobalIdentifier ?? []);
        member._symbol = global + '.' + member._symbol;
        member.Parent = module;
    }

    private struct ModuleContent()
    {
        public List<StaticFieldBuilder> fields = [];
        public List<BaseFunctionBuilder> functions = [];
        public List<StructureBuilder> structs = [];
        public List<TypeDefinitionBuilder> typedefs = [];
    }
}
