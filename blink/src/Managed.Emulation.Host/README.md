# Managed host implementation

`VirtualFileSystem` is an implemented and independently tested part of the
per-instance host contract. It owns a private Linux-path namespace, clones image
bytes, exposes immutable image files and a bounded mutable file layer, and never
reads or writes the host filesystem. Instances share no file nodes, cursors or
descriptors. Required parent directories derive from the image; directory
creation, symlinks, unlink/rename and metadata mutation are not implemented.

Implemented behavior includes read/write access, create/exclusive/truncate/
append, EOF and short I/O, zero-filled sparse gaps, seek, shared-cursor descriptor
duplication, close, basic stat, bounded image bytes, aggregate writable bytes,
descriptor exhaustion and disposal. Error results use explicit Linux numbers.
Traversal above the virtual root is denied. Intermediate non-directories and
trailing slashes on files fail, and failed open/descriptor allocation cannot
create or truncate a file.

`FileAccessMode` is a typed host contract, not the Linux open-flag wire encoding.
Native callback marshalling, guest buffer validation, errno assignment, and
integration with the core's descriptor/socket/standard-stream tables are still
pending. This module alone does not start a guest and is not a security boundary.
Socket readiness and cancellation are outside this file contract.

Run `blink/scripts/test-host-files.sh` from any directory through its full path.
The separate consumer checks image ownership, two-instance isolation, shared
duplicate offsets, exact bytes including sparse zeroes, short writes at quota,
resource-failure atomicity, append, invalid paths, overflow, and repeated cleanup.
All four assertion groups pass under Linux x64 JIT and NativeAOT. This is P4
host feasibility evidence; P4's actual guest-service startup gate remains open.
