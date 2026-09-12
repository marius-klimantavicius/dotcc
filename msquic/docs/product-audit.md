# Product dependency and provenance audit

Run `python3 msquic/scripts/audit-product.py` after the final regeneration and
separate public consumer campaign. The audit binds the pinned archive, staged
source bytes and authored overlays, exact compiler snapshot, generated manifests,
P2 receipts and API profile. It requires actual compiled source revision metadata
and the full raw/optimized JIT/trimmed NativeAOT consumer receipt.

Authored host, facade and reused provider sources are scanned for native imports,
loaders, process creation, reflection/code generation and native QUIC/TLS facade
use. Translated core source is checked separately from the generic embedded libc
runtime. That runtime's native import declarations are retained in the report;
its presence alone does not establish that a particular wrapper is reachable.
Actual public consumer output files must match the recorded hashes. NativeAOT ELF
NEEDED entries are inventoried and reject native MsQuic/picotls/TLS backends.
System libraries used by the .NET BCL are not alternate product TLS state machines.

The final audit passes with zero violations and zero missing prerequisites against
all 32 fresh public-consumer cases. The receipt at
`artifacts/product-audit/results.json` binds exact source, generated, tool and
published-binary hashes, dependency manifests and ELF inventories. This audit
does not replace runtime recovery/lifetime tests, performance measurements,
final SQLite regression, or unavailable platform runs.
