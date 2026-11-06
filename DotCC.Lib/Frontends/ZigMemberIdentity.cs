using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DotCC.Ir;

namespace DotCC.Frontends;

internal enum ZigMemberKind { Enum, Field, Variant }

internal sealed record ZigMemberIdentity(string SourceName, string TargetName, int Order, CType? PayloadType);

/// <summary>Source identities belong to one import invocation. Target spellings never become lookup keys.</summary>
internal sealed class ZigMemberIdentityRegistry
{
    private const string Prefix = "__zig_member_";
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private readonly Dictionary<(string Owner, ZigMemberKind Kind), IReadOnlyList<ZigMemberIdentity>> _members = new();

    internal IReadOnlyList<ZigMemberIdentity> Register(string owner, ZigMemberKind kind,
        IEnumerable<(string Name, CType? Payload)> members)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        var targets = new Dictionary<string, string>(StringComparer.Ordinal);
        var result = new List<ZigMemberIdentity>();
        foreach (var (name, payload) in members)
        {
            Validate(name);
            if (!names.Add(name)) throw new CompileException($"zig: duplicate {kind} member '{name}' in '{owner}'");
            var target = Encode(owner, name);
            var clr = target.TrimStart('@');
            if (Reserved(owner, clr) || !targets.TryAdd(clr, name))
                throw new CompileException($"zig: target member collision for '{owner}.{name}' ({target})");
            result.Add(new(name, target, result.Count, payload));
        }
        // Registrations may be revisited by lazy resolution, but cannot silently change identity/order.
        if (_members.TryGetValue((owner, kind), out var previous))
        {
            if (!previous.SequenceEqual(result))
                throw new CompileException($"zig: conflicting member identities for '{owner}' ({kind})");
            return previous;
        }
        _members.Add((owner, kind), result);
        return result;
    }

    internal static string Decode(byte[] bytes)
    {
        string name;
        try { name = Utf8.GetString(bytes); }
        catch (DecoderFallbackException)
        {
            throw new CompileException("zig: unsupported member name with invalid UTF-8 bytes: " + Convert.ToHexString(bytes));
        }
        Validate(name);
        return name;
    }

    private static void Validate(string name)
    {
        if (name.Length == 0 || name.Contains('\0'))
            throw new CompileException("zig: member names must be nonempty and NUL-free");
        try { _ = Utf8.GetByteCount(name); }
        catch (EncoderFallbackException) { throw new CompileException("zig: unsupported member name with invalid UTF-8 source bytes"); }
    }

    private static bool Reserved(string owner, string name) => name == "value__"
        || name == EmitHelpers.Id(owner).TrimStart('@') || name == AggregateIdentities.Token(owner).TrimStart('@')
        || name.StartsWith("__bf", StringComparison.Ordinal)
        || name.StartsWith("__dotcc", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("__IA_", StringComparison.Ordinal)
        || name.StartsWith("__zig_", StringComparison.Ordinal) && !name.StartsWith(Prefix, StringComparison.Ordinal);

    private static string Encode(string owner, string source)
    {
        var bytes = Utf8.GetBytes(source);
        bool ascii = (char.IsAsciiLetter(source[0]) || source[0] == '_')
            && source.All(c => char.IsAsciiLetterOrDigit(c) || c == '_');
        string target = ascii && !source.StartsWith(Prefix, StringComparison.Ordinal) && !Reserved(owner, source)
            ? source : Prefix + Convert.ToHexString(bytes);
        // Stay below Id's generic length bound; hash the complete original bytes.
        if (target.Length > 192) target = Prefix + "H_" + Convert.ToHexString(SHA256.HashData(bytes));
        if (Reserved(owner, target)) target = Prefix + "O_" + Convert.ToHexString(SHA256.HashData(bytes));
        // `default` is a valid Zig member. The shared C name policy leaves it
        // bare for generated value-initialization syntax, so escape it here.
        return target == "default" ? "@default" : EmitHelpers.EscapeKeyword(target);
    }
}
