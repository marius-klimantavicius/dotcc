# Valid pinned-image loader qualification

`run.py --assembly-receipt <qualified core objects/receipt.json>` loads only the
hash-pinned static service fixture. The runner checks the immutable guest,
archive, profile, object and compiler hashes. It neither constructs nor accepts
mutated executable inputs. No guest instructions execute.

The authored adapter calls unchanged `LoadProgram`, with the normal overlay
initialization and a private `/bin/service` image explicitly marked executable.
HostMemory, signal actions, exit callbacks and file readers are initialized
before upstream allocation. Guest resource limits are initialized after
NewSystem and before NewMachine. Existing private environment, identity, IO,
directory and sleep owners are bound by the separate managed consumer.

The valid native oracle and translated adapter load two fresh machines. They
compare all file-backed segment bytes using a digest derived from the pinned
file, check every BSS byte for zero, inspect every segment page's guest R/W/X
permissions, and verify the RW/NX aligned stack, argv, environment and auxiliary
vector. Entry, program-header address/count and entry bytes are compared too.
Random bytes must match that load's actual `elf.rng`, rather than another
process's independently sampled entropy. The table of expected segments is
inserted into the authored C source from the pinned ELF; that exact source and
the original template are separately hashed in the receipt.

The managed link is explicitly derived: it retains the qualified core objects,
replaces the HostMemory implementation with its qualified software-protection
version, and adds this adapter object. It uses the original physical canonical
header tree, preserving anonymous C aggregate identities. The frozen memory
header already declares every interface needed by the loader. All retained
object producer receipts and new source/object/compiler hashes are recorded;
this is not a claim that all core translation units were freshly emitted.
Generated C# is never rewritten. Raw and optimized libraries are independently
built for JIT and NativeAOT, with the whole translated assembly rooted for AOT.

FreeMachine destroys each loaded machine. Normal completion then runs registered
exit callbacks while services and mappings remain valid, ends callback/signal
state and discards memory. The consumer verifies only its three standard
private descriptors remain and mapping charges are zero. Exceptional unwinds
skip normal exit callbacks and dispose boundaries before the entire process is
discarded. No upstream static cache is reused after disposal.

This is a loader-state gate for one static valid image. It does not qualify
service instruction execution, dynamic ELF interpreters, relocations, malformed
inputs or reusable process-global core state. Host backing remains ordinary RW
C memory with software protection metadata; guest PTEs enforce permissions.

Qualified receipt: `artifacts/elf-loading/attempt-e1zyczdz/receipt.json`.
Native and raw/optimized JIT/AOT outputs match exactly for both loads: 26,074
file-backed bytes, 394,960 zero BSS bytes, 318 segment permission checks, and
an aligned RW/NX stack. Entry is `0x4015c4`, PHDR `0x400040`, with six program
headers and 13 nonterminal auxiliary entries. Cleanup checks passed in all four
managed processes. The first emission attempt retained a parser diagnostic for
multiple authored string-array declarations; splitting those declarations
resolved the harness syntax without changing upstream or compiler sources.
