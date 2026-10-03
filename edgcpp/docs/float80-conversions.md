# Fixed-width binary80 conversions

Float80 now uses UInt128 arithmetic for conversions as well as its accepted
comparison, addition/subtraction, multiplication and division operations. There
are no BigInteger paths left in this class. Integer inputs normalize through
the common nearest-even packer; double inputs preserve their exact represented
value. Narrowing to Half, float and double explicitly handles shifts at and
beyond the integer width, ties, carry, subnormal rounding and overflow. Checked
integer outputs truncate first and then enforce each target range, including
negative fractional values that truncate to zero and the signed128 minimum.

Two final JIT/NativeAOT checks in
`artifacts/campaign/float80-conversion-final-001/results.json` compare36,000
native x87 records,100,000 independent BigInteger narrowing cases,2,000 raw
integer-range cases across ten target types and explicit sign/bit-boundary
grids. One million warmed conversion groups allocate zero managed bytes in
each runtime. Candidate runs and failed investigations remain recorded. Eight
shared provider checks in `musl-provider-locale-conversions-001` verify the
composed runtime with185 selected C source units in C/C++ static/instance modes.

This preserves the established numerical interpretation of raw encodings; it
does not promise NaN payload propagation, dynamic rounding modes or hardware
exception flags. Other library helpers and binary128 still need their own
allocation audit. Execution evidence is Linux x64 only.
