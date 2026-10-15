The fifth unchanged SQLite diagnostic candidate exposed 65 mixed integer
compound-assignment compiler errors and one conditional record value passed
as a C# `ref` argument. A narrow public-reader probe selected the 46 affected
bodies. Its identity-deduplicated graph facts are inventory, not an execution
schedule or a source-language reanalysis.

The public facts are preserved at
`/mnt/storage/roslynobj-csharp-20261015/t3/compiler5-operator-facts.json`,
SHA-256 `4a1e2e336e5d9cedeb8a88c31f581b3d75d891c951280415c40e21fd5c627127`.
The corresponding compiler diagnostic JSON has SHA-256
`4a5cf7f3e2ff445c5c35bad64e88076c704d68562985667fe4c726ae8baeea6b`.

Pristine stock [il.c](/tmp/roslynobj-edg-upstream-20261012/src/il.c:18042), lines 18042–18107,
`compound_assignment_operation_type`, supplies the computation contract:
ordinary compound operand two already has the computation type selected by
EDG. Shift, pointer and fixed-point cases have separate rules. The public
`OperationTypeKind` reports only the broad type kind; it does not establish
integer width or signedness.

The new ordinary integer adapter therefore converts the captured old value
to the retained RHS type, performs the operation there, and converts the
result to the selected destination storage type. It admits natural ordinary
signed and unsigned 8/16/32/64-bit storage with a retained signed or unsigned 32/64-bit
computation representation. Boolean and saturating forms are excluded. The
existing pointer, shift, volatile, bitfield and native admission guards stay
in force. Place capture, old-value load, RHS effects and store follow the
retained operand constraints. Already valid same-representation compounds
and established pure narrow/int32 helpers keep their existing simple path.

The failing `yy_reduce` record occurrence is exactly `eok_assign`: its
destination is an lvalue, its conditional RHS and both branches are prvalues,
and its selected record storage is size 16/alignment 8. It is not `eok_bassign`.
The planner materializes the selected branch value into an ordinary temporary
with identical captured record identity, size and alignment before passing
storage to the existing copy helper. Branch prefixes remain inside their
selected branch. Source-sized partial block copies and differing record
representations remain unsupported.

The [focused fixture and checker](../tests/CompoundConversions/README.md)
exercise 22 exact observations: the failing integer representation families,
once-only place/RHS effects, both conditional record branches, and existing
narrow helpers. Frozen snapshot 11 compiled successfully; native and generated
JIT/NativeAOT outputs matched all 22 observations. Its generated source SHA-256 is
`454b40dc8f78708bd1bef4951a85d3d7b9678feaaeee13560e1e08107657604d`.
Measured phase evidence is under
`/mnt/storage/roslynobj-csharp-20261015/t3/compound-conversions-*-11`.
The next full candidate compile remains a separate coordinator gate;
this bounded activation does not establish full SQLite execution.


The [measured bounded receipt](../acceptance/compound-conversions/report.json)
records 11 passing phases with source/input freezes, exact generated files and
byte-identical native/JIT/NativeAOT output. The [native constant/address receipt](../acceptance/native-constants/report.json)
separately records nine passing phases for all-ones native constants and canonical
function addresses stored through data-pointer and routine-pointer views.
