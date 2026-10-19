The sample packs a directory into a single standard zstd frame containing a
small multi-file archive, then restores file names and bytes. Compression and
decompression stream through reusable owning contexts. A temporary disk file
holds the archive records, so the archive size does not determine managed heap
use. The sample preserves regular files and relative names, not permissions,
timestamps, empty directories or symbolic-link identity.

Generate the translated product with `bash zstd/scripts/translate.sh`, then:

```bash
dotnet run -c Release --project zstd/samples/ManagedConsumer -- pack input output.dzar.zst
dotnet run -c Release --project zstd/samples/ManagedConsumer -- unpack output.dzar.zst restored
dotnet run -c Release --project zstd/samples/ManagedConsumer -- train sample-records archive.dict
dotnet run -c Release --project zstd/samples/ManagedConsumer -- pack input output.dzar.zst archive.dict
dotnet run -c Release --project zstd/samples/ManagedConsumer -- unpack output.dzar.zst restored archive.dict
```

Extraction requires a new directory. Dictionary files contain upstream zstd
dictionary bytes; they can also be used by native zstd. The frame itself is
standard zstd; decoding it with the native CLI produces the archive record
stream, not an individual source file. `--self-test WORK` creates a four-file
fixture and checks exact filenames and SHA-256 digests after plain and dictionary
archive restores. The campaign runs this workflow in every selected form/mode.
