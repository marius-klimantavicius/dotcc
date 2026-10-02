//type:fp
//options_all:--c++20
//remark:Internal error in record_substitution_for_type during constraint checking
// 7/9/26   [EDGcpfe/28704,EDGcpfe/28938]
//
// Internal error in record_substitution_for_type during constraint checking
//
// Previously, constraint checking involving a failed variable template
// specialization could result in an error type entry being inserted into the IL
// tree.  In some configurations, that error type entry reached the mangling
// routines and triggered an assertion failure in record_substitution_for_type.
template<typename T> constexpr bool v = true;
template<typename T> struct B { };
struct C {
  template<typename T, bool = v<T>> operator T();
};
template<typename T> concept X = requires (T t) { B{t}; };
static_assert(!X<C>);  // Previously triggered an assertion failure.
                       // Now okay.
