# Lambda static entry provenance

Captureless lambda conversion creates a distinct static entry that forwards to
its original call operator. The generated entry has no ordinary source
parameter declarations: EDG creates fresh formal variables when it defines the
forwarding body. Generic lambda instantiations can also have different symbol
visibility for the entry and the selected call operator.

The exporter records `lambda_entry_origins` with the selected call operator,
original closure type, original static routine signature, and exact creation
proof. `lambda_entry_calls` identifies the call observed while EDG builds the
entry. Source names, source signature, physical ABI, and generated formal roles
remain separate. No fake ordinary source function or parameter is introduced.

The verifier checks original return/parameter/exception semantics against the
selected call operator, removes exactly its closure receiver from the static
entry signature, and verifies the body forwards every unchanged formal once.
The inserted receiver must be the exact typed null constant. Indirect return
storage and indirect by-value parameters keep their original ABI roles. The
entry neither invents another record copy nor takes ownership of result storage.

Stock anchors are `class_decl.c::generate_lambda_conversion_function`
(`sfk_lambda_entry_point` and `variant.lambda_call_operator`),
`lower_il.c`'s call to `define_default_version_of_routine`, and
`lower_init.c::define_default_version_of_routine` (fresh parameters, optional
result buffer, null receiver, selected target call).

The fixture `duckdb_lambda_static_entry.cpp` covers ordinary, generic, noexcept,
throwing, indirect-result and indirect-parameter callbacks. Final profile52 acceptance passes **36 checks** with native125 and managed146:
native C++ and stock C at O0/O2, source-hidden metadata verification/linking,
production runtime binding, JIT execution, NativeAOT execution, and six resealed
negative cases. The negatives substitute the original closure, selected target,
or static signature; delete forwarding metadata; replace the null receiver by a
well-typed nonnull pointer expression; and add a second use of a forwarded formal.
Receipt: `/mnt/storage/dotcc-edg-campaign-20261003/dotobj-v52-lambda-125-146-001/results.json`.

Acceptance also exercises a C# precedence correction: a lifted function-pointer
callee is emitted as a local identifier, preserving its existing single
evaluation. Parenthesizing that identifier before a parenthesized argument can
otherwise be parsed as a cast. Earlier failed evidence remains retained; it does
not count as acceptance.
