# Composed scalar call chain

The native oracle uses two live C++ owners and two live concrete inputs. The
checker supplies four separate aligned 56-byte buffers with 8-byte guards on
both sides. It initializes selected ordinary scalar fields and address slots;
vptr and padding bytes remain opaque sentinels. Full before/after comparisons
are within each buffer, with no cross-representation vptr or padding equality.
This is borrowed layout-byte execution, without C++ construction, virtual
execution, ownership or lifetime claims. The zero-live scalar slot is protocol.

The frozen native matrix has input kinds 5/201 and 128/255. Each tuple uses four
pointer/branch configurations and repeats each invocation, for 16 complete
comparisons of result, all 24 values, all events, overflow/live, four unchanged
objects/buffers and two getter-to-payload address identities. Null branches
produce no guard calls; direct nonnull branches expose expectation and guard
effects. Native JSON supplies the exact event/observation authority. Actual
unsigned generated reads are checked again after input mutations.

The supplied provenance pins source/header/native oracle, captured input and
public facts. The outer receipt must bind the compiled generated project and
checker; supplied hashes alone do not establish that connection.

The binding partial uses actual `composed-scalar-generated-2` names. It measures
borrowed owner/input view extents 56/8, complete proxy 24/8, derived/storage 16/8,
refcount 8/8 and access 1/1, plus field/base offsets, sequential-prefix alignment,
typed array stride and actual addresses. The owner payload is at 24 and proxy
selector at 16. The unrelated polymorphic owner prefix is not emitted as a
complete generated C++ carrier.

Two additional guarded raw owners exercise actual getter/delegation/conversion
and composed scalar bodies across null, first input, second input and null.
They check pointer identity, fresh signed8 Boolean 0/1, composed unsigned-byte
0/128/255/0, per-phase direct versus base/no-call effects, both full-width prefix
fields and suffix, inert pointer cookies and whole-owner/guard immutability.
The four original buffers must remain unchanged throughout these physical hooks.
No native application, reader/reflection fallback or reconstructed C++ body is
permitted. The bound checker is ready for a coordinator build; it has not been
built or run by its author.

After build:
`Checks NATIVE_OBSERVATIONS_JSON CAPTURE_PROVENANCE_JSON`.
