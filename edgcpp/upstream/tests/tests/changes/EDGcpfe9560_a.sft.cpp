//type:fp
//options_all:--g++
//remark:[4.1] GNU C++ compatibility: __restrict on typedefs and template parameters
// 2/23/09  [EDGcpfe/9560]
//
// Restrict-qualified member functions
//
// Previously, in C++ modes that accept a restrict qualifier on a member function
// declaration, the front end treated that qualifier as participating in the type
// of the member function.  Now, this is not the case.
//
// Note that unlike the standard cv-qualifiers on member function declarations
// (which are qualifiers for the type pointed to by this), "restrict" is a top-
// level qualifier for the hidden "this" parameter.  The new behavior is thus a
// correction that treats qualifiers on "this" like qualifiers on other
// parameters.
//
// This change involves an IL CHANGE: The "restrict" qualifier (TQ_RESTRICT) is
// now recorded in the new "this_qualifiers" field of the routine type
// supplement for the member function, and not in its "qualifiers" field.
//
// This change also has an ABI implication: Previously, the "restrict" qualifier
// on member functions was erroneously encoded into the mangled name of the
// member function.  That is no longer the case.
//
// 2/23/09  [EDGcpfe/9560]
//
// GNU C++ compatibility: __restrict on typedefs and template parameters
//
// In GNU C++ mode, the front end now ignores __restrict on typedef types if the
// underlying type of the typedef is not a pointer type.  Similarly, __restrict
// is ignored on template parameters if the corresponding argument is not a
// pointer type.
//
// A remark is issued in such cases.
typedef float F;
F __restrict f;  // Now accepted in g++ mode.
