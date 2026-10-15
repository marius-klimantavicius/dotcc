using System.Globalization;
using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private readonly Dictionary<string, CppType> hostLayouts = new(StringComparer.Ordinal);

        private void RequireHostLayouts(NativeImport import)
        {
            // Provider ABI policy names actual libc public object types. The
            // expected quantities/fields always come from the retained model.
            var entry = import.Binding.EntryPoint;
            if (entry is "pthread_mutex_init" or "pthread_mutex_destroy" or "pthread_mutex_lock" or "pthread_mutex_trylock" or "pthread_mutex_unlock")
                RequireHostPointee(import.Function.Signature.Parameters[0].Type, "pthread_mutex_t");
            if (entry is "pthread_mutexattr_init" or "pthread_mutexattr_destroy" or "pthread_mutexattr_settype")
                RequireHostPointee(import.Function.Signature.Parameters[0].Type, "pthread_mutexattr_t");
            if (entry == "pthread_mutex_init") RequireHostPointee(import.Function.Signature.Parameters[1].Type, "pthread_mutexattr_t");
            if (entry == "pthread_create")
            {
                RequireHostPointee(import.Function.Signature.Parameters[0].Type, "pthread_t");
                RequireHostPointee(import.Function.Signature.Parameters[1].Type, "pthread_attr_t");
            }
            if (entry == "pthread_join") RequireHostLayout(import.Function.Signature.Parameters[0].Type, "pthread_t");
        }
        private void RequireHostPointee(CppType type, string nativeType)
        {
            if (Unwrap(type) is not PointerType { IsReference: false } pointer)
                throw Limit("A selected host storage parameter lacks its ordinary pointer ABI.", "host-layout");
            RequireHostLayout(pointer.PointeeType, nativeType);
        }
        private void RequireHostLayout(CppType type, string nativeType)
        {
            var raw = Unwrap(type);
            if (type.Size != raw.Size || type.Alignment != raw.Alignment || raw.Size is null || raw.Alignment is null)
                throw Limit("A selected host storage type lacks an exact effective/raw layout.", "host-layout");
            ValidateHostLayoutTree(raw, new HashSet<CppType>());
            if (hostLayouts.TryGetValue(nativeType, out var existing) &&
                (existing.Size != raw.Size || existing.Alignment != raw.Alignment))
                throw Limit("Selected host declarations require different storage representations for one native provider type.", "host-layout");
            hostLayouts.TryAdd(nativeType, raw);
        }
        private static void ValidateHostLayoutTree(CppType type, HashSet<CppType> seen)
        {
            type = Unwrap(type);
            if (!seen.Add(type)) return;
            if (type.Size is null || type.Alignment is null) throw Limit("Native provider storage has unavailable size/alignment.", "host-layout");
            if (type is not RecordType record) return;
            if (record.Layout is not { IsComplete: true } layout || record.Bases.Count != 0 || record.HasVirtualFunctions || record.HasVirtualBases)
                throw Limit("Native provider storage needs its complete retained ordinary layout.", "host-layout");
            foreach (var field in layout.Fields)
            {
                if (field.Field.IsBitField || field.Offset is null || field.Field.Name is not { } name ||
                    name.Length == 0 || !name.All(c => char.IsAsciiLetterOrDigit(c) || c == '_') || char.IsAsciiDigit(name[0]))
                    throw Limit("Native provider layout field cannot be checked with an exact ordinary C ABI member assertion.", "host-layout");
                ValidateHostLayoutTree(field.Field.Type, seen);
            }
        }
        private void AppendHostLayoutAssertions(StringBuilder source)
        {
            source.AppendLine("#include <pthread.h>")
                .AppendLine("_Static_assert(sizeof(int) == 4 && _Alignof(int) == 4, \"logical errno int ABI\");");
            foreach (var item in hostLayouts) AppendHostLayoutAssertion(source, item.Value, item.Key);
        }
        private static void AppendHostLayoutAssertion(StringBuilder source, CppType type, string nativeType)
        {
            type = Unwrap(type);
            source.Append("_Static_assert(sizeof(").Append(nativeType).Append(") == ").Append(type.Size?.ToString(CultureInfo.InvariantCulture)).AppendLine(", \"captured provider storage size\");");
            source.Append("_Static_assert(_Alignof(").Append(nativeType).Append(") == ").Append(type.Alignment?.ToString(CultureInfo.InvariantCulture)).AppendLine(", \"captured provider storage alignment\");");
            if (type is not RecordType record) return;
            foreach (var field in record.Layout!.Fields)
            {
                var name = field.Field.Name!;
                source.Append("_Static_assert(offsetof(").Append(nativeType).Append(", ").Append(name).Append(") == ")
                    .Append(field.Offset?.ToString(CultureInfo.InvariantCulture)).AppendLine(", \"captured provider member offset\");");
                AppendHostLayoutAssertion(source, field.Field.Type, $"__typeof__((({nativeType} *)0)->{name})");
            }
        }
    }
}
