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

`partial-array.cpp` is the preserved first array candidate, with an automatic
`ArrayElement items[3]`. Its default constructor records an attempted id, throws
only for the selected failure id, and records live ownership/completion only
after that check. The fixture runs success first and then failure at id 2,
retaining the same five-value protocol, 64-event buffer and prior-live evidence.
The successful body observes all three element ids. Capture/public facts and
independent native observations are recorded separately; actual helper signatures,
completion counts, cleanup arrays and state writes govern the projection. This adds no heap allocation, inheritance, nested throw,
class exception payload or native unwinding.

`partial-array-prefixes.cpp` preserves the original element definitions and
`array_path`, extending only the observation plumbing and fixture sequence to
success and failures at ids 1, 2 and 3. Markers 900, 901 and 902 separate those
paths. Nine values record each result/live pair and prior live count last.
The original `partial-array.cpp` and its capture/evidence remain unchanged.
Actual capture/native comparison and generated JIT/NativeAOT now establish empty,
one-element and two-element completed prefixes with reverse partial cleanup. The shared
exception checker takes explicit counts `9 3` for this variant.

`integer-rethrow.cpp` exercises a separate bounded ownership activation. One
initial integer throw reaches an inner typed handler; changing that handler's
copied parameter to 99 precedes an operandless rethrow to an outer typed handler.
Five values record the inner original, copied mutation, outer original, final
live count and prior live count. Distinct tracked objects surround the first
throw, inner handler and outer try. The source records application observations
only; one primary payload, handling-entry ownership and final release require
separate captured/runtime evidence. Capture, production CLI and native/JIT/NativeAOT
execution now pass; see [the rethrow proof](../../../docs/T5-RETHROW.md). This does not activate a new exception thrown from a handler, class
payloads or native unwinding.


The prefix variant now passes production CLI and native/JIT/NativeAOT complete
observations, including zero completed elements and reverse destruction of two
completed elements. See [the array proof](../../../docs/T5-PARTIAL-ARRAY.md).
Original source/capture/native evidence is preserved separately. The genuine
`ordinary-vector-names.cpp` negative has no array cleanup context and remains
unsupported despite its import spellings.

`noexcept-termination.cpp` is a separate default-termination candidate. Its
borrowed 68-int shared map records outer object 1, boundary-local object 3 and
throwing-leaf object 2. A normal mode shares the same bodies; raising mode must
stop at the captured noexcept boundary rather than reach the caller's typed
catch. Capture/public facts and native observations are recorded. The selected
pristine runtime policy unwinds before terminating, while the separate GCC
observation does not unwind; preserve that differential. Production CLI/JIT/NativeAOT now prove the selected cleanup/signal/marker
contract. See [the selected contract](../../../docs/T5-TERMINATION.md). Uncaught
program-entry policy, custom handlers and native unwinding remain separate.
