//type:fp
//options_all:--c++11
//remark:[4.12] User-defined conversions for array bounds
// 8/5/16   [EDGcpfe/17050]
//
// User-defined conversions for array bounds
//
// With the C++11 constexpr feature, an array bound can be obtained through a
// user-defined conversion from a class type.  In C++11, obtaining an array
// bound from an expression involves a "converted constant expression" (this is
// a specific term from the C++11 standard) with a destination type of size_t.
// The front end previously only retained the "conversion to size_t" part,
// ignoring constraints implied by the "converted constant expression" part.
// In particular, conversions from floating-point types should not be considered.
//
// Previously, both conversion operators were considered valid, equally-good
// matches to produce an array bound of type size_t; an ambiguous conversion
// error was emitted.  Now, the "operator float" candidate is discarded since a
// float-to-size_t conversion is not permitted in this context; "operator bool"
// is therefore successfully selected.
struct S {
  constexpr S() {};
  constexpr operator float() const { return 1.0; }
  constexpr operator bool() const { return true; }
};
float x[S()];  // Previously ambiguous.  Now okay.
