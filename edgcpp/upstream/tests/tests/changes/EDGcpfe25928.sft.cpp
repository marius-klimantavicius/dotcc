//type:fp
//options_all:--c++20 --gnu_version=120200
//remark:[6.5] Incorrect substitution of built-in operators in constraints
// 1/12/23  [EDGcpfe/25928]
//
// Incorrect substitution of built-in operators in constraints
//
// The front end previously did not correctly substitute the constraint on the
// non-defaulted destructor of U, which resulted in a spurious error claiming that
// the destructor of U is deleted (which would be true if only the defaulted
// destructor were declared).  That is now fixed.
struct X { ~X(); };
template<typename...> union U {};
template<typename T, typename... Unused>
union U<T, Unused...> {
  ~U() = default;
  ~U() requires(__has_trivial_destructor(U<>));
  T bp;
};
U<X> var;  // Previously an error.  Now okay.
