using System.Formats.Asn1;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

namespace Managed.S2nTls.Crypto;

public static unsafe partial class OpenSslAbi
{
    private sealed class NameGrouping(List<int> sets) { public readonly List<int> Sets = sets; }
    private static readonly ConditionalWeakTable<NameState, NameGrouping> NameGroupings = new();

    private static NameGrouping NameGroups(NameState name) => NameGroupings.GetValue(name, value =>
    {
        var reader = new AsnReader(value.Der, AsnEncodingRules.DER).ReadSequence();
        var sets = new List<int>();
        for (int group = 0; reader.HasData; group++)
        {
            var rdn = reader.ReadSetOf();
            while (rdn.HasData) { rdn.ReadEncodedValue(); sets.Add(group); }
        }
        if (sets.Count != value.Entries.Count) throw new InvalidOperationException();
        return new NameGrouping(sets);
    });

    private static byte[] NameValueDer(int type, ReadOnlySpan<byte> bytes)
    {
        if (type is not (12 or 19 or 20 or 22 or 26 or 28 or 29 or 30))
            throw new NotSupportedException("Only the selected raw ASN.1 name string types are implemented.");
        var writer = new AsnWriter(AsnEncodingRules.DER);
        writer.WriteOctetString(bytes);
        byte[] encoded = writer.Encode();
        encoded[0] = (byte)type;
        return encoded;
    }

    private static (byte[] Der, byte[] Canonical) EncodeName(List<(string Oid, nint Value)> entries, List<int> sets)
    {
        var writer = new AsnWriter(AsnEncodingRules.DER);
        writer.PushSequence();
        using var canonical = new MemoryStream();
        for (int index = 0; index < entries.Count;)
        {
            writer.PushSetOf();
            var normalized = new AsnWriter(AsnEncodingRules.DER);
            normalized.PushSetOf();
            int group = sets[index];
            do
            {
                var entry = entries[index];
                var native = (AsnStringNative*)entry.Value;
                byte[] raw = NameValueDer(native->Type, Input(native->Data, (ulong)native->Length));
                writer.PushSequence(); writer.WriteObjectIdentifier(entry.Oid); writer.WriteEncodedValue(raw); writer.PopSequence();
                var state = Get<AsnStringState>((void*)entry.Value);
                if (state.Text is string text && native->Type is 12 or 19 or 20 or 22 or 26 or 28 or 30)
                {
                    byte[] utf8 = Encoding.UTF8.GetBytes(text);
                    var cleaned = new List<byte>();
                    bool Space(byte value) => value is 9 or 10 or 11 or 12 or 13 or 32;
                    int first = 0, last = utf8.Length;
                    while (first < last && Space(utf8[first])) first++;
                    while (last > first && Space(utf8[last - 1])) last--;
                    for (int i = first; i < last; i++)
                    {
                        byte value = utf8[i];
                        if (Space(value)) { cleaned.Add(32); while (i + 1 < last && Space(utf8[i + 1])) i++; }
                        else cleaned.Add(value is >= 65 and <= 90 ? (byte)(value + 32) : value);
                    }
                    raw = NameValueDer(12, cleaned.ToArray());
                }
                normalized.PushSequence(); normalized.WriteObjectIdentifier(entry.Oid); normalized.WriteEncodedValue(raw); normalized.PopSequence();
                index++;
            } while (index < entries.Count && sets[index] == group);
            writer.PopSetOf(); normalized.PopSetOf(); canonical.Write(normalized.Encode());
        }
        writer.PopSequence();
        return (writer.Encode(), canonical.ToArray());
    }

    private static void SetNameEncoding(NameState name, byte[] der, byte[] canonical)
    {
        byte* copy = (byte*)NativeMemory.Alloc((nuint)Math.Max(1, der.Length));
        if (copy == null) throw new OutOfMemoryException();
        der.CopyTo(Output(copy, der.Length));
        nint previous = name.DerPointer;
        // Production has no mutation entry points and initializes these cached
        // fields once. The isolated test assembly supplies the real mutable
        // X509_NAME boundary while preserving its object/borrowed-entry identity.
        Unsafe.AsRef(in name.Der) = der;
        Unsafe.AsRef(in name.Canonical) = canonical;
        Unsafe.AsRef(in name.DerPointer) = (nint)copy;
        NativeMemory.Free((void*)previous);
    }

    public static void* X509_NAME_new()
    {
        NameState? state = null;
        try
        {
            state = new NameState(X509Name.GetInstance(new DerSequence()));
            void* handle = Add(state); state = null; return handle;
        }
        catch { state?.Dispose(); Error(); return null; }
    }

    public static void X509_NAME_free(void* name) { try { Remove(name); } catch { Error(); } }

    public static int X509_NAME_add_entry_by_NID(void* name, int nid, int type, byte* bytes, int length, int location, int set)
    {
        nint added = 0;
        try
        {
            var state = Get<NameState>(name);
            if (!Nids.TryGetValue(nid, out var info)) throw new ArgumentException();
            if (length < 0)
            {
                if (bytes == null) throw new ArgumentException();
                length = 0; while (bytes[length] != 0) length = checked(length + 1);
            }
            byte[] data = Input(bytes, (ulong)length).ToArray();
            _ = NameValueDer(type, data);
            string? text = type switch
            {
                12 => new UTF8Encoding(false, true).GetString(data),
                28 => new UTF32Encoding(true, false, true).GetString(data),
                30 => new UnicodeEncoding(true, false, true).GetString(data),
                19 or 20 or 22 or 26 => Encoding.Latin1.GetString(data),
                _ => null
            };
            added = Asn(type, data, text);
            var entries = new List<(string Oid, nint Value)>(state.Entries);
            var grouping = NameGroups(state);
            var groups = new List<int>(grouping.Sets);
            int loc = location < 0 || location > entries.Count ? entries.Count : location;
            bool increment = set == 0;
            int target;
            if (set == -1)
            {
                if (loc == 0) { target = 0; increment = true; }
                else target = groups[loc - 1];
            }
            else target = loc == entries.Count ? (loc == 0 ? 0 : groups[loc - 1] + 1) : groups[loc];
            entries.Insert(loc, (info.Oid, added)); groups.Insert(loc, target);
            if (increment) for (int i = loc + 1; i < groups.Count; i++) groups[i]++;
            var encoded = EncodeName(entries, groups);
            state.Entries.EnsureCapacity(entries.Count); grouping.Sets.EnsureCapacity(groups.Count);
            SetNameEncoding(state, encoded.Der, encoded.Canonical);
            state.Entries.Clear(); state.Entries.AddRange(entries);
            grouping.Sets.Clear(); grouping.Sets.AddRange(groups);
            added = 0;
            return 1;
        }
        catch { if (added != 0) Remove((void*)added); Error(); return 0; }
    }

    private static NameState CopyName(NameState name)
    {
        var copy = new NameState(X509Name.GetInstance(new DerSequence()));
        try
        {
            foreach (var entry in name.Entries)
            {
                var value = Get<AsnStringState>((void*)entry.Value);
                var native = (AsnStringNative*)entry.Value;
                copy.Entries.Add((entry.Oid, Asn(native->Type, Input(native->Data, (ulong)native->Length), value.Text)));
            }
            NameGroupings.Add(copy, new NameGrouping(new List<int>(NameGroups(name).Sets)));
            SetNameEncoding(copy, (byte[])name.Der.Clone(), (byte[])name.Canonical.Clone());
            return copy;
        }
        catch { copy.Dispose(); throw; }
    }

    public static int X509_set_subject_name(void* certificate, void* name)
    {
        nint owned = 0;
        NameState? copy = null;
        try
        {
            if (certificate == null) return 0;
            var state = Get<CertState>(certificate);
            copy = CopyName(Get<NameState>(name));
            owned = (nint)Add(copy); copy = null;
            lock (state.Cache)
            {
                if (state.Cache.Remove("subject", out nint previous))
                {
                    state.Borrowed.Remove(previous); Remove((void*)previous);
                }
                state.Cache.Add("subject", owned); state.Borrowed.Add(owned); owned = 0;
            }
            // As in the selected native setter, the original certificate DER
            // and signature cache remain intact. The subject object is copied,
            // owned by the certificate, and read by the actual s2n CN loader.
            return 1;
        }
        catch { copy?.Dispose(); if (owned != 0) Remove((void*)owned); Error(); return 0; }
    }
}
