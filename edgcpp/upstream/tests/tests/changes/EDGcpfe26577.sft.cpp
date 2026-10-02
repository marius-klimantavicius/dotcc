//type:fp
//options_all:--c++20
//remark:[6.7] Abort on pack expansion in concept template argument list
// 7/26/24  [EDGcpfe/26577,EDGcpfe/27396]
//
// Abort on pack expansion in concept template argument list
//
// Previously, the front end failed to correctly expand parameter packs when
// disambiguating between a type-constraint and a concept-id, which could result
// in an error type entry being inserted into the IL tree.  In some
// configurations, that error type entry reached the mangling routines and
// triggered an assertion failure in record_substitution_for_type.
// with --c++20:
template<typename ...> concept X = true;
template<typename> struct C {};
template<typename ... Us>
void f() {
  X<C<Us> ...>;  // Previously triggered an assertion failure.  Now okay.
}
template void f<>();
