//type:fn
//options_all:--c++20
//remark:[6.8] Adjustments to handling of identifiers in lambda headers
// 8/8/25   [EDGcpfe/21022,EDGcpfe/21246,EDGcpfe/24784,EDGcpfe/25500]
//
// Handling of captures in a lambda header
//
// The C++ standardization paper P2579R0 (and its predecessor, P2036R3) changes
// the way captures are handled in a lambda header.  In particular:
// These changes were voted into the standard as a defect report (DR), which means
// they apply to all language modes.  However, it appears MSVC, GCC, and Clang do
// not yet implement this feature and the front end therefore approximates their
// behavior in its corresponding modes.  The front end now also diagnoses lambda
// parameters and (C++20) explicit template parameters whose names conflict with
// explicitly-specified captures.
//
// 8/6/25   [EDGcpfe/24784,EDGcpfe/25500]
//
// Adjustments to handling of identifiers in lambda headers
//
// The front end now diagnoses (with a discretionary error) lambda parameters and
// (C++20) explicit lambda template parameters whose names conflict with that of
// an explicit capture of that lambda.
//
// Furthermore, the front end now also implements the changes in the C++
// standardization committee's paper P2579R0 (adopted as a DR), which causes
// some additional uses of captured variables to become "const".
auto lm = [x = 1](int x) { return x; };  // Now an error.
