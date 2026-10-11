using System.Text;
using System.Text.Json;

namespace DotObj;

internal static class CatalogGenerator
{
    public static void Write(string catalog,string output,string namespaceName,string className,string? entry,bool shared,
        Action<WeakReference>? audit=null)
    {
        if(Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any())throw new IOException("Output directory must be empty");
        Directory.CreateDirectory(output);
        using var plan=new CatalogEmissionPlan(catalog,className,shared);plan.PrepareNames();var arenas=plan.PrepareStorage();
        using var sourceStream=File.Create(Path.Combine(output,"source-map.json"));
        using var macroStream=File.Create(Path.Combine(output,"macro-map.json"));
        using var lifetimeStream=File.Create(Path.Combine(output,"lifetime-map.json"));
        using var source=new Utf8JsonWriter(sourceStream,new JsonWriterOptions{Indented=true});
        using var macros=new Utf8JsonWriter(macroStream,new JsonWriterOptions{Indented=true});
        using var lifetimes=new Utf8JsonWriter(lifetimeStream,new JsonWriterOptions{Indented=true});
        foreach(var writer in new[]{source,macros,lifetimes})
        {
            writer.WriteStartObject();writer.WriteNumber("formatVersion",1);writer.WriteString("profile","scalar-control-v66");
            if(ReferenceEquals(writer,source)){writer.WriteString("capture","scalar-declarations-before-lowering");writer.WriteBoolean("prelowerSource",false);}
            else if(ReferenceEquals(writer,macros)){writer.WriteString("capture","preprocessor-definition-events");writer.WriteBoolean("macroDefinitions",true);writer.WriteBoolean("macros",false);}
            else
            {
                writer.WriteString("capture","lexical-activations-and-observed-cpp-cleanup-states");writer.WriteBoolean("cleanupStates",true);
                writer.WriteBoolean("sourceLifetimes",true);writer.WriteBoolean("lexicalCleanups",true);writer.WriteBoolean("lifetimes",false);
            }
            writer.WriteStartArray("objects");
        }
        bool globals=false,programArguments=false;int entries=0;long units=0;
        foreach(var unit in plan.Units())
        {
            WeakReference released=CatalogUnitReader.ConsumeUnit(unit,obj=>
            {
                obj.WriteSourceMap(source);obj.WriteMacroMap(macros);
                plan.Report("objects",unit.UnitId,new{path=obj.Path,unitId=obj.UnitId,primarySource=obj.PrimarySource,semanticDigest=obj.Digest});
                if(plan.Scalar("SELECT unit_id FROM selected_units WHERE unit_id=?",unit.Id) is null)return;
                var view=new Linker(obj,unit,plan);
                var emitter=new Emitter(view,namespaceName,className,shared,plan,unit);
                if(!globals){emitter.WriteCatalogGlobals(output,arenas);globals=true;}
                emitter.WriteCatalogUnit(output,lifetimes);
                emitter.WriteCatalogReport();
                if(entry is not null){var found=emitter.WriteCatalogEntry(output,entry);entries+=found.Count;programArguments|=found.Arguments;}
            });
            audit?.Invoke(released);units++;
            Console.WriteLine(JsonSerializer.Serialize(new{status="unit-generated",unitId=unit.UnitId,units,managedBytes=GC.GetTotalMemory(false)}));
        }
        if(!globals)throw new InvalidDataException("Catalog has no selected primary unit");
        if(entry is not null && entries!=1)throw new InvalidDataException("Entry must identify one externally defined function: "+entry);
        foreach(var writer in new[]{source,macros,lifetimes}){writer.WriteEndArray();writer.WriteEndObject();writer.Flush();}
        string project="""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
                <Nullable>enable</Nullable>
                <ImplicitUsings>disable</ImplicitUsings>
                <CheckForOverflowUnderflow>false</CheckForOverflowUnderflow>
                <IsAotCompatible>true</IsAotCompatible>
                <Deterministic>true</Deterministic>
            """+"\n    <OutputType>"+(entry is null?"Library":"Exe")+"</OutputType>\n"+
            (entry is null?"":"    <StartupObject>__DotObjEntry</StartupObject>\n")+"  </PropertyGroup>\n</Project>\n";
        File.WriteAllText(Path.Combine(output,"Translated.csproj"),project,new UTF8Encoding(false));
        plan.WriteReport(Path.Combine(output,"link-report.json"),namespaceName,className,programArguments,shared);
        plan.WriteSelection(Path.Combine(output,"link-selection.json"));plan.Complete();
    }
}
