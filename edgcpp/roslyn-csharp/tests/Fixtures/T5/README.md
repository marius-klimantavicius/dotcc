# Focused C++ lifetime inputs

`constructor-failure.cpp` exercises bounded constructor-body failure after the
normal and integer-exception slices. It uses one typed `catch (int)`, one source throw,
and one member whose constructor and destructor are `noexcept`. The containing
constructor first completes that member, then either returns or throws.

An ordinary scalar canary precedes the member. The constructor, successful body
and containing destructor record its value through events or the return value.
The capture must establish the actual member offset and cleanup storage; the
source declaration is not a layout or cleanup-order recipe.

The existing five-value protocol is unchanged: normal result, live count after
normal completion, failing result, live count after failure, and prior live
count. The fixture does not reset the live count. Its caller should invoke it
twice and compare all values and events with an independent native observation,
checking zero live objects and overflow after each invocation. Only the member
owns live-count bookkeeping; the containing constructor/destructor add events.

Capture/public inspection and production CLI/JIT/NativeAOT complete observations
now pass; see [the proof](../../../docs/T5-CONSTRUCTOR-FAILURE.md). Actual retained
calls/state writes and receiver-relative member offset 4 drive cleanup of the
completed member when the containing constructor fails, without invoking the
incomplete containing object's destructor. Arrays, failed-member
construction, inheritance, allocation, rethrow and native unwinding remain
outside this candidate.
