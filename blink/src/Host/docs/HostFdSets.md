# Private fd-set storage

The selected upstream `SysSelect` implementation manipulates host fd sets before
calling the private poll boundary. The generic compatibility header's one-word
fd_set and no-op helpers cannot represent this path. The authored header selects
an actual128-byte record with1024 bits, and ordinary C functions implement zero,
set, clear and membership operations. Valid calls preserve errno. Null records
return EFAULT; descriptors outside0–1023 return EINVAL without changing storage.
Callers still own valid readable/writable C records.

Fresh qualification against the recovered compiler passes at
`artifacts/host-fd-sets/attempt-0ko66tt9/receipt.json`: native Linux macros and
authored functions agree over19456 bitmap cases with digest7606779652642019554.
Separate C objects pass raw/optimized JIT and NativeAOT, including function
pointers, record copies, invalid bounds, and two independent TLS records with
cached C addresses across compacting GC. The newer fixed-address storage change
in fde3e7e resolves the earlier pointer-movement failure retained at
`attempt-acp3rz3c`; the authored record was not reshaped to hide it.

The binding manifest includes this header and implementation for fresh core
profiles. Full translated guest select execution remains a separate open gate.
