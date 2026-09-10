using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace DotCC.PostProcess;

internal static class InPlaceWriter
{
    private sealed record Edit(string Source, string Target, string Hash, string Temporary, string Backup);
    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    // The optional replacement operation allows tests to inject an I/O failure
    // or cancellation during a multi-file commit, without filesystem races.
    internal static int Write(CSharpCompilation original, CSharpCompilation rewritten,
        IReadOnlyDictionary<string, string> sourceHashes, CancellationToken token,
        Action<string, string>? replaceFile = null)
    {
        replaceFile ??= (source, target) => File.Move(source, target, overwrite: true);
        var originals = original.SyntaxTrees.ToArray();
        var trees = rewritten.SyntaxTrees.ToArray();
        if (originals.Length != trees.Length || originals.Where((tree, i) => tree.FilePath != trees[i].FilePath).Any())
            throw new InvalidOperationException("In-place rewriting must preserve source order and paths.");
        var comparison = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var targets = sourceHashes.Keys.ToDictionary(path => path, SnapshotPaths.Canonical, StringComparer.Ordinal);
        var expected = new Dictionary<string, string>(sourceHashes, StringComparer.Ordinal);
        var edits = new List<Edit>();
        var replaced = new List<Edit>();
        var retainedBackups = new HashSet<string>(StringComparer.Ordinal);

        void VerifyInput(string path)
        {
            if (!comparison.Equals(SnapshotPaths.Canonical(path), targets[path])
                || Hash(File.ReadAllBytes(path)) != expected[path])
                throw new IOException("Input source changed during processing: " + path);
        }
        void VerifyInputs() { foreach (var path in expected.Keys) VerifyInput(path); }

        try
        {
            VerifyInputs();
            var serialized = rewritten;
            var changedTargets = new HashSet<string>(comparison);
            for (int i = 0; i < trees.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                var text = trees[i].GetRoot(token).ToFullString();
                if (text == originals[i].GetRoot(token).ToFullString()) continue;
                var path = trees[i].FilePath;
                var target = targets[path];
                if (!changedTargets.Add(target))
                    throw new IOException("Multiple changed inputs refer to the same source file: " + path);
                var attributes = File.GetAttributes(target);
                if ((attributes & FileAttributes.ReadOnly) != 0)
                    throw new UnauthorizedAccessException("Cannot update read-only source: " + path);
                var before = File.ReadAllBytes(target);
                if (Hash(before) != sourceHashes[path]) throw new IOException("Input source changed during processing: " + path);
                var encoding = originals[i].GetText(token).Encoding ?? new UTF8Encoding(false);
                var preamble = encoding.GetPreamble();
                bool hasPreamble = preamble.Length != 0 && before.AsSpan().StartsWith(preamble);
                var encoded = encoding.GetBytes(text);
                var bytes = hasPreamble ? preamble.Concat(encoded).ToArray() : encoded;
                using var stream = new MemoryStream(bytes, writable: false);
                var source = SourceText.From(stream, encoding, originals[i].GetText(token).ChecksumAlgorithm, canBeEmbedded: true);
                serialized = serialized.ReplaceSyntaxTree(trees[i], CSharpSyntaxTree.ParseText(source,
                    (CSharpParseOptions)trees[i].Options, path, token));
                var stem = Path.Combine(Path.GetDirectoryName(target)!, ".dotcc-postprocess-" + Guid.NewGuid().ToString("N"));
                var edit = new Edit(path, target, Hash(bytes), stem + ".tmp", stem + ".bak");
                edits.Add(edit);
                using (var output = new FileStream(edit.Temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    output.Write(bytes);
                    output.Flush(flushToDisk: true);
                }
                File.SetAttributes(edit.Temporary, attributes);
                if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(edit.Temporary, File.GetUnixFileMode(target));
                File.Copy(target, edit.Backup, overwrite: false);
                if (Hash(File.ReadAllBytes(edit.Backup)) != sourceHashes[path])
                    throw new IOException("Input source changed while preparing replacement: " + path);
            }

            if (edits.Count != 0) CondInliner.CheckErrors(serialized, "Serialized", token);
            token.ThrowIfCancellationRequested();
            VerifyInputs();
            foreach (var edit in edits)
            {
                token.ThrowIfCancellationRequested();
                VerifyInput(edit.Source);
                replaceFile(edit.Temporary, edit.Target);
                replaced.Add(edit);
                expected[edit.Source] = edit.Hash;
            }
            token.ThrowIfCancellationRequested();
            VerifyInputs();
            return edits.Count;
        }
        catch (Exception failure)
        {
            var rollbackErrors = new List<string>();
            foreach (var edit in replaced.AsEnumerable().Reverse())
            {
                try
                {
                    // Never overwrite a user's edit made after our replacement.
                    if (Hash(File.ReadAllBytes(edit.Target)) != edit.Hash)
                        throw new IOException("Source was edited after replacement: " + edit.Target);
                    File.Move(edit.Backup, edit.Target, overwrite: true);
                }
                catch (Exception rollback) when (rollback is IOException or UnauthorizedAccessException)
                {
                    retainedBackups.Add(edit.Backup);
                    rollbackErrors.Add(rollback.Message + "; original retained at " + edit.Backup);
                }
            }
            if (rollbackErrors.Count != 0)
                throw new IOException("In-place processing failed and rollback was incomplete.\n" + string.Join("\n", rollbackErrors), failure);
            throw;
        }
        finally
        {
            foreach (var edit in edits)
            {
                File.Delete(edit.Temporary);
                if (!retainedBackups.Contains(edit.Backup)) File.Delete(edit.Backup);
            }
        }
    }
}
