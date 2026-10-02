//type:fn
//options_all:--c++20 -tused -A
//remark:[6.1] Defaulted comparison operators
// 3/24/20  [EDGcpfe/22351,EDGcpfe/22462]
//
// Defaulted comparison operators
//
// The front end now implements the changes of the C++ standardization committee's
// paper P2002R1 regarding the handling of defaulted comparison operators (a C++20
// feature; see the entry for EDGcpfe/20014).  Among the effects of this change is
// a new diagnostic when declaring a defaulted operator== or operator<=> that
// would directly invoke a non-constexpr function.
struct S { friend bool operator==(S const&, S const&); };
struct X: S {
  friend constexpr bool operator==(X const&, X const&) = default;
    // Now an error because the constexpr operator== for X would invoke
    // the non-constexpr operator== for S.
};
