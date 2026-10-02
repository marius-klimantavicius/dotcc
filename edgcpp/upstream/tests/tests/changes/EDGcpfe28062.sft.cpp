//type:fp
//options_all:--clang_v 190100
//remark:[6.8] Clang compatibility: Type traits helpers and member function types
// 4/4/25   [EDGcpfe/28062]
//
// Clang compatibility: Type traits helpers and member function types
//
// Clang 19.x accepts member function cv-qualifiers and ref-qualifiers for the
// type operands of type traits helpers.  The front end emulates that in
// corresponding modes (GCC and MSVC had been accepting such constructs already
// and the front end emulated that as well).
static_assert(!__is_pod(int() &));  // Previously a syntax error in all
                                    // Clang modes.  Now okay when
                                    // clang_version >= 190000.
