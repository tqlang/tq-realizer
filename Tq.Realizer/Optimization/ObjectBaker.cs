using System.Diagnostics;
using Tq.Realizer.Builder.ProgramMembers;
using Tq.Realizer.Builder.References;
using Tq.Realizer.Core.Configuration.LangOutput;

namespace Tq.Realizer.Optimization;

public static class ObjectBaker
{
    public static void BakeTypeMetadata(
        ILanguageOutputConfiguration config,
        StructureBuilder[] structs,
        TypeDefinitionBuilder[] typedefs)
    {
        
        BakeTypedefs(config, typedefs);
        BakeStructs(config, structs);
        
        PackStructures(structs);
    }

    private static void BakeStructs(
        ILanguageOutputConfiguration configuration,
        StructureBuilder[] structs)
    {
        List<StructureBuilder> toiter = structs.ToList();
        Dictionary<StructureBuilder, (uint a, uint s)> baked = [];

        // FIXME cyclic dependencies will result in 
        // infinite loop

        looooop:
        if (toiter.Count == 0) goto end;
        for (var i = 0; i < toiter.Count; i++)
        {
            var cur = toiter[i];
            uint minAlig = configuration.NativeIntegerSize; // Type table ptr
            uint size = configuration.NativeIntegerSize;
            
            if (cur.Extends != null)
            {
                if (!baked.TryGetValue(cur.Extends, out var shit)) goto hardbreak;
                minAlig = Math.Max(minAlig, cur.Extends.Alignment!.Value);;
                size += cur.Extends.Length!.Value;
            }
            
            foreach (var j in cur.Fields)
            {
                j.Offset = size;
                switch (j.Type)
                {
                    case IntegerTypeReference @it:
                        var intAlig = (it.Bits ?? configuration.NativeIntegerSize).AlignForward(configuration.MemoryUnit);
                        
                        j.Size = it.Bits ?? configuration.NativeIntegerSize;
                        j.Alignment = intAlig;
                        break;
                    
                    case NodeTypeReference @nt:
                        switch (nt.TypeReference)
                        {
                            case StructureBuilder @struc:
                                if (!baked.TryGetValue(struc, out var shit)) goto hardbreak;
                                
                                j.Size = struc.Length;
                                j.Alignment = struc.Alignment;
                                break;
                            
                            case TypeDefinitionBuilder @typedef:
                                j.Size = configuration.NativeIntegerSize;
                                j.Alignment = configuration.NativeIntegerSize;
                                break;
                        }
                        break;
                    
                    case ReferenceTypeReference:
                        j.Size = configuration.NativeIntegerSize;
                        j.Alignment = configuration.NativeIntegerSize;
                        break;
                    
                    default: throw new UnreachableException();
                }
                size += j.Size!.Value;
                minAlig = Math.Max(minAlig, j.Alignment!.Value);
            }
            
            cur.Alignment = minAlig;
            cur.Length = size;
            baked.Add(cur, (minAlig, size));
            toiter.RemoveAt(i);
            
            hardbreak: ;
        }
        goto looooop;
        end: ;
    }
    
    
    private static void BakeTypedefs(
        ILanguageOutputConfiguration config,
        TypeDefinitionBuilder[] typedefs)
    {
        
    }

    private static void PackStructures(StructureBuilder[] structs)
    {
        foreach (var i in structs)
        {
            i.Fields.Sort((a, b)
                => -(int)(a.Alignment!.Value - b.Alignment!.Value));

            uint off = 0;
            foreach (var f in i.Fields)
            {
                var alignedOff = off.AlignForward(f.Alignment!.Value);
                f.Offset = alignedOff;
                off = alignedOff + f.Size!.Value;
            }
        }
    }
}
