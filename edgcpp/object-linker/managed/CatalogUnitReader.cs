using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace DotObj;

// A catalog locator never grants permission to trust a changed input. This
// reader rechecks exact file bytes and the complete object-format boundary on
// each materialization and returns no source graph from a completed callback.
internal sealed class CatalogUnitReader : IDisposable
{
    private readonly Sqlite catalog;
    public CatalogUnitReader(string path)
    {
        catalog=new Sqlite(path);
        using var info=catalog.Prepare("SELECT format_version,verifier_build,status FROM catalog_info WHERE id=1");
        if(!info.Read() || info.Integer(0)!=LinkCatalog.FormatVersion ||
            info.Text(1)!=typeof(LinkCatalog).Assembly.ManifestModule.ModuleVersionId.ToString() ||
            info.Text(2) is not("imported-not-resolved" or "selected-not-generated"))
            throw new InvalidDataException("Catalog unit reader requires a complete catalog from this reader build");
    }
    private static (long Size,string Hash) Identity(string path)
    {
        if(File.Exists(path+"-wal") || File.Exists(path+"-journal"))
            throw new InvalidDataException("Catalog input has an active SQLite journal");
        using var input=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
        return(input.Length,Convert.ToHexStringLower(SHA256.HashData(input)));
    }
    public IEnumerable<CatalogUnitDescriptor> Units()
    {
        using var units=catalog.Prepare("SELECT id,path,file_sha256,byte_length,logical_sha256,unit_id,primary_source,source_language,provider,input_ordinal,function_count,verified_body_count FROM units ORDER BY unit_id");
        while(units.Read())yield return new(units.Integer(0),units.Text(1),units.Text(2),units.Integer(3),units.Text(4),
            units.Text(5),units.Text(6),units.Text(7),units.Integer(8)==1,units.Integer(9),units.Integer(10),units.Integer(11));
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public WeakReference WithUnit(CatalogUnitDescriptor descriptor,Action<ObjectFile> consume)
        =>ConsumeUnit(descriptor,consume);
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static WeakReference ConsumeUnit(CatalogUnitDescriptor descriptor,Action<ObjectFile> consume)
    {
        var expected=(descriptor.ByteLength,descriptor.FileHash);
        if(Identity(descriptor.Path)!=expected)throw new InvalidDataException("Catalog input bytes changed after import: "+descriptor.Path);
        using var obj=new ObjectFile(descriptor.Path);
        if(obj.UnitId!=descriptor.UnitId || obj.Digest!=descriptor.Digest || obj.PrimarySource!=descriptor.PrimarySource ||
            obj.SourceLanguage!=descriptor.Language || obj.Functions.Count!=descriptor.FunctionCount)
            throw new InvalidDataException("Catalog unit identity differs from its verified object");
        var lifetime=new WeakReference(obj);
        consume(obj);
        if(Identity(descriptor.Path)!=expected)throw new InvalidDataException("Catalog input changed during emission: "+descriptor.Path);
        return lifetime;
    }
    public IEnumerable<long> SelectedFunctions(long unit)
    {
        using var functions=catalog.Prepare("SELECT w.local_id FROM winners w JOIN functions f ON f.unit_id=w.unit_id AND f.entity_id=w.local_id WHERE w.kind='function' AND w.selected=1 AND w.unit_id=? ORDER BY f.sort_key");
        functions.Bind(unit);while(functions.Read())yield return functions.Integer(0);
    }
    public void WriteMaps(string output)
    {
        if(Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any())
            throw new IOException("Catalog map output directory must be empty");
        Directory.CreateDirectory(output);
        using var sourceStream=File.Create(Path.Combine(output,"source-map.json"));
        using var macroStream=File.Create(Path.Combine(output,"macro-map.json"));
        using var source=new Utf8JsonWriter(sourceStream,new JsonWriterOptions{Indented=true});
        using var macros=new Utf8JsonWriter(macroStream,new JsonWriterOptions{Indented=true});
        foreach(var writer in new[]{source,macros})
        {
            writer.WriteStartObject();writer.WriteNumber("formatVersion",1);writer.WriteString("profile","scalar-control-v64");
            writer.WriteStartArray("objects");
        }
        foreach(var unit in Units())WithUnit(unit,obj=>{obj.WriteSourceMap(source);obj.WriteMacroMap(macros);});
        foreach(var writer in new[]{source,macros}){writer.WriteEndArray();writer.WriteEndObject();}
    }
    public void Dispose()=>catalog.Dispose();
}
