//remark:auto-template-param deduction
//options:--c++20;fp

// EDGcpfe/24771

  enum E { e };
  template<auto&> int g();                             // (1)
  template<E...> constexpr bool g() { return true; }   // (2)
  static_assert(g<e>());  // Previously an error.  Now okay.

