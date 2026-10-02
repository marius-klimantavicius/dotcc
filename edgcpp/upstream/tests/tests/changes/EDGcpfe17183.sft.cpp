//type:fn
//options_all:--c++14
//remark:[4.12] C++14 constexpr: Accessibility and address comparisons
// 5/13/16  [EDGcpfe/17183]
//
// C++14 constexpr: Accessibility and address comparisons
//
// Comparing pointers to fields of the same class type does not produce a
// constant expression if the fields have different accessibility.  The C++14
// interpreter previously used the wrong accessibility to check this criterion
// in some cases.
//
// This is now fixed.
struct S {
  constexpr S(): i(1), j(2), k(3) {};
  constexpr bool nonconstant() const { return &i < &k; }
  int i;
  int j;
private:
  int k;
};

constexpr S s{};
constexpr bool b = s.nonconstant();
   // Previously erroneously accepted.  Now an error (because k is private
   // and i is not).
