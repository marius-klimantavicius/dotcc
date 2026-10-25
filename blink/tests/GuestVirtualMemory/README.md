# Explicit virtual reservation budgets

The ordinary static Linux fixture queries AS/DATA limits and exercises two
policies with 32 MiB backing: the unchanged default rejects a 512 MiB reservation
and permits a small allocation; an explicit 1 GiB virtual budget permits a sparse
512 MiB reservation, writes its first/last pages, and unmaps it. Both policies
require exact output and cleanup through the public API.

```sh
python3 -B blink/tests/GuestVirtualMemory/run-managed.py --prepare-only
python3 -B blink/tests/GuestVirtualMemory/run-managed.py \
  --clean-delivery blink/artifacts/campaign/current-threaded.json --aot --runs 1
```

The second command requests 16 cases: raw/processed, JIT/NativeAOT hosts, both
public modes and both policies. Linux references use matching per-process
`prlimit` AS/DATA settings. The runner captures both producer closures before
builds and records all process-group/disposal outcomes. Sparse virtual capacity
does not raise the resident backing budget. This fixture does not qualify
CoreCLR, writable regular shared files, or native Blink's registry race.
