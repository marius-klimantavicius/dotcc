//type:fp
//remark:[4.12] Ambiguous member access not detected during SFINAE processing
// 6/3/16   [EDGcpfe/17038]
//
// Ambiguous member access not detected during SFINAE processing
//
// The front end previously did not detect certain ambiguous member accesses when
// rescanning a member selection during SFINAE processing.  It instead proceeded
// with the first member it found in the ambiguity set.  This could trigger
// spurious errors.
//
// Previously, this triggered an ambiguity error for "T().x" because candidate (1)
// was not discarded during SFINAE processing.  This is now fixed.
struct B1 { int x; };
struct B2 { int x; };
struct D: public B1, B2 {};
template<typename T> decltype(T().x) f(T*);  // (1)
template<typename T> char f(T);
static_assert(sizeof(f((D*)0)) == 1, "Unexpected!");
