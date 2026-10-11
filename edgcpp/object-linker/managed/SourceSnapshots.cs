using System.IO.Compression;
using System.Security.Cryptography;

namespace DotObj;

internal sealed partial class ObjectFile
{
    private void VerifySourceSnapshots()
    {
        var identities = new Dictionary<string, string>(StringComparer.Ordinal);
        using var sources = database.Prepare("SELECT available,sha256,bytes,path,codec,decoded_byte_length FROM source_files");
        byte[] buffer = new byte[65536];
        while (sources.Read())
        {
            if (sources.Integer(0) == 0)
            {
                if (!sources.IsNull(1) || !sources.IsNull(2) || !sources.IsNull(4) || !sources.IsNull(5))
                    throw new InvalidDataException("Unavailable source snapshot has encoded contents");
                continue;
            }
            if (sources.Integer(0) != 1 || sources.IsNull(1) || sources.IsNull(2) || sources.IsNull(4) || sources.IsNull(5))
                throw new InvalidDataException("Source snapshot lacks its encoding contract");
            string digest = sources.Text(1), path = sources.Text(3), codec = sources.Text(4);
            long length = sources.Integer(5);
            byte[] encoded = sources.Bytes(2);
            if (length < 0 || length > int.MaxValue)
                throw new InvalidDataException("Source snapshot decoded length is outside supported bounds");
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            if (codec == "raw")
            {
                if (encoded.LongLength != length)
                    throw new InvalidDataException("Raw source snapshot length mismatch");
                hash.AppendData(encoded);
            }
            else if (codec == "zlib")
            {
                // RFC 1950, DEFLATE only, no externally supplied dictionary.
                if (encoded.Length < 6 || encoded.LongLength >= length || (encoded[0] & 15) != 8 ||
                    (encoded[0] >> 4) > 7 || ((encoded[0] << 8) + encoded[1]) % 31 != 0 || (encoded[1] & 32) != 0)
                    throw new InvalidDataException("Invalid compressed source snapshot header or extent");
                using var input = new SourceSnapshotInput(encoded);
                using var decoder = new ZLibStream(input, CompressionMode.Decompress, leaveOpen: true);
                long decoded = 0;
                for (;;)
                {
                    // Read one byte past the declared boundary to reject excess
                    // output, without allocating the claimed decoded length.
                    int take = (int)Math.Min(buffer.Length, length - decoded + 1);
                    int count = decoder.Read(buffer, 0, take);
                    if (count == 0) break;
                    decoded += count;
                    if (decoded > length)
                        throw new InvalidDataException("Compressed source snapshot exceeds its decoded length");
                    hash.AppendData(buffer, 0, count);
                }
                if (decoded != length || input.Position != encoded.LongLength)
                    throw new InvalidDataException("Compressed source snapshot length, termination or trailing bytes mismatch");
            }
            else throw new InvalidDataException("Unsupported source snapshot codec");
            if (Convert.ToHexStringLower(hash.GetHashAndReset()) != digest)
                throw new InvalidDataException("Source snapshot digest mismatch");
            if (identities.TryGetValue(path, out string? prior) && prior != digest)
                throw new InvalidDataException("Include occurrences disagree on source snapshot identity");
            identities[path] = digest;
        }
    }

    // ZLibStream may buffer input and accepts underlying EOF independently of
    // the DEFLATE end marker. Limit tail reads to one byte and supply a sentinel
    // after the encoded frame: a valid decoder must finish before reading it.
    // This proves exact frame consumption without a second DEFLATE parser.
    private sealed class SourceSnapshotInput(byte[] encoded) : Stream
    {
        private long position;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => encoded.LongLength + 1;
        public override long Position { get => position; set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override int Read(Span<byte> buffer)
        {
            if (buffer.Length == 0 || position == Length) return 0;
            if (position == encoded.LongLength)
            {
                buffer[0] = 0;
                ++position;
                return 1;
            }
            long prefix = Math.Max(0, encoded.LongLength - 64);
            int count = position < prefix ? (int)Math.Min(buffer.Length, prefix - position) : 1;
            encoded.AsSpan((int)position, count).CopyTo(buffer);
            position += count;
            return count;
        }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
