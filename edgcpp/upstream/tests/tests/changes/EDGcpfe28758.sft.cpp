//type:fp
//options_all:-A --c++23
//remark:C++23: Constexpr constructor calling non-constexpr constructors
// 3/31/26  [EDGcpfe/28758]
//
// C++23: Constexpr constructor calling non-constexpr constructors
//
// In C++23 mode, the front end no longer diagnoses constexpr constructors that
// call non-constexpr constructors as part of subobject initialization (as long
// as that constexpr constructor is not invoked in a context that requires a
// constant).
//
// This change in behavior is the result of the C++ standardization committee's
// paper P2448R2.
struct S {
  S(int);
  constexpr S(): S(0) {}  // Previously an error.  Now okay.
};
S s1;            // Okay.
