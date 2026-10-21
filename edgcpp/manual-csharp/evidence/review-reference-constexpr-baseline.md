# Independent reference constexpr baseline review

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. This is a
source/design review for the next implementation after accepted scalar-reference
checkpoint `7c16cf48dbcdb57c070ca90625ebec4b703c66a4`. It is not implementation
acceptance. No reviewer implementation edits, builds/tests, compiler/native
execution or Git operations were performed.

## Actual upstream contracts inspected

- `interpret.c:70–241`: separate automatic/temporary/static storage and maps;
  automatic scopes and full-expression temporaries have different cleanup;
  lifetime extension and allocation sequence/live-set checks are explicit.
- `interpret.c:1104–1146,2323–2375`: call frames have a parent, result location,
  entry allocation sequence and return/control state. Result transport is not
  the same operation as creating a callee object.
- `interpret.c:25080–25167` (`do_glvalue_to_prvalue`): actual loads validate
  dereference eligibility, symbolic runtime data, live allocation, null,
  volatile, initialization and active object state before copying a value.
- `interpret.c:27321–27362`: address_of/reference_to transport glvalue addresses;
  indirect/ref_indirect either transport an address or perform a load according
  to result category. Address formation must not eagerly read a mutable object.
- `interpret.c:22700–22835` (`map_param_to_arg`, `unmap_or_restore_param`,
  `finish_call_work`): parameter mapping remembers/restores any prior recursive
  activation, and completion restores mappings and pops the frame.
- `interpret.c:22952–23090,23227–23265` (`process_call_work`): all arguments are
  evaluated before mapping callee parameters, explicitly to avoid recursive
  calls changing the meaning of later arguments. Parameters receive the
  allocation sequence of the callee body even when argument buffers are
  physically allocated in the caller context.
- `interpret.c:9356–9424,9628–9636`: evaluate the return expression into the
  frame result location, then activate return/unwinding. Returning a scalar load
  before cleanup differs from transporting an address whose referent expires.
- `interpret.c:6587ff,6679ff,33330–33389`: symbolic runtime/unknown addresses
  and mutable temporary constants require contextual treatment. Optional folding
  must not freeze mutable temporary storage and substitute that address into a
  later manifestly constant evaluation. The C++20/P2280 conditional branches
  require native profile evidence before claiming broader unknown-reference use.

## Finite integration checks required

1. A stable mutable-global address can be transported without treating its
   runtime value as constant. Loading it must fail the required constant gate.
   Const qualification alone must not make a runtime-initialized object usable.
2. By-value parameters own separate callee cells. Reference parameters and
   results preserve caller object/temporary identity. Recursive/nested calls
   cannot remap the caller's symbols while arguments are still being evaluated.
3. Caller argument temporaries survive the callee until their full expression;
   callee parameters, locals and return-created temporaries expire appropriately.
   Alias binding preserves the original lifetime owner and never re-extends it.
4. Declaration-time optional failure for parameter-dependent locals is not an
   invocation-time failure. Conversely, a cached successful declaration result
   must not bypass active frame storage or resurrect an expired object.
5. Discarding a glvalue avoids its lvalue-to-rvalue load but still executes a
   call producing that glvalue. The old blanket LValue expression-statement
   skip is insufficient once reference-returning calls are interpreted.
6. Early returns and nested failures unwind all storage/work/frame state;
   repeated Evaluate calls reset budgets and do not retain previous TU graphs.
   Retained public facts must not expose evaluator-owned mutable cells.
7. Required versus optional evaluation, invalid binding versus runtime lifetime
   hazard, and Unsupported syntax remain distinct. Assignment/control flow,
   dependent references, classes and source-level pointers gain no accidental
   support from an internal address representation.

The existing 95-group checkpoint is the regression baseline. Approval requires
review of actual new source/assertions and matching held Release, managed,
NativeAOT, exact native controls and coupled source witnesses. The full PLAN
continues beyond this bounded extension.
