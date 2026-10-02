//type:fp
//options_all:--c++20
//remark:[6.3] Constant-evaluation of pointer and pointer-to-member casts
// 5/13/21  [EDGcpfe/22472,EDGcpfe/23539,EDGcpfe/23840]
//
// Constant-evaluation of pointer and pointer-to-member casts
//
// The front end was previously unable to constant-evaluate certain pointer and
// pointer-to-member casts: That is now fixed.
struct S {
  int const *const *p;
  constexpr S(int **p) : p(p) {}
};
constexpr S sp(nullptr);  // Previously an error because the conversion
                          // in the mem-initializer was not supported.
                          // Now okay.
