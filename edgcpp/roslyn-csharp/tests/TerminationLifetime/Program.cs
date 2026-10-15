using System.IO.MemoryMappedFiles;
using System.Text.Json.Nodes;

if (args.Length != 2 || args[1] is not ("0" or "1"))
    throw new InvalidOperationException("Checks EXISTING_MAP_PATH MODE(0|1)");
var mode = args[1] == "0" ? 0 : 1;
const int slotCount = 68;
const int mappingBytes = slotCount * sizeof(int);
using var file = new FileStream(Path.GetFullPath(args[0]), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
if (file.Length != mappingBytes) throw new InvalidOperationException("The existing mapped observation file must contain exactly 272 bytes");
using var mapping = MemoryMappedFile.CreateFromFile(file, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true);
using var view = mapping.CreateViewAccessor(0, mappingBytes, MemoryMappedFileAccess.ReadWrite);
unsafe
{
    byte* viewBase = null;
    var acquired = false;
    try
    {
        view.SafeMemoryMappedViewHandle.AcquirePointer(ref viewBase);
        acquired = true;
        var slots = (int*)(viewBase + view.PointerOffset);
        for (var index = 0; index < slotCount; ++index)
            if (slots[index] != 0) throw new InvalidOperationException("Each child requires a fresh zero-initialized observation map");
        var result = global::Translated.Program.fixture(slots, mode);
        if (mode != 0) throw new InvalidOperationException("The raising noexcept fixture returned instead of terminating");
        if (result != 1 || slots[0] is < 0 or > 64 || slots[1] != 0 || slots[2] != 0 || slots[3] != 1)
            throw new InvalidOperationException("Normal noexcept fixture result or mapped invariants failed");
        view.Flush();
        Console.WriteLine(new JsonObject
        {
            ["mode"] = mode, ["result"] = result, ["count"] = slots[0],
            ["overflow"] = slots[1], ["live"] = slots[2], ["resultMarker"] = slots[3]
        }.ToJsonString());
    }
    finally
    {
        if (acquired) view.SafeMemoryMappedViewHandle.ReleasePointer();
    }
}
