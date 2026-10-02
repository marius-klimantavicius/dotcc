//type:fp
//options_all:--microsoft
//remark:[4.11] Microsoft/Clang/GNU compatibility: Use of static array data member in template
// 1/15/16  [EDGcpfe/16402]
//
// Microsoft/Clang/GNU compatibility: Use of static array data member in template
//
// In GNU and Clang modes, an argument of a function call that is a reference to
// a static data member of incomplete array type is now considered a dependent
// argument.  Furthermore, in non-template-dependent contexts, referring to a
// static data member of a class template instance whose type is an incomplete
// array type triggers the instantiation of the initializer of that data member
// (if applicable) so that the bound of the array can be determined.  That
// causes the front end to accept in some modes cases that previously elicited
// errors.
//
// Line (2) is normally invalid because x has a nondependent type that cannot be
// unified with the parameter declaration in (1).  The case was already accepted
// in GNU and Clang modes that defer prototype instantiations.  Now it is also
// accepted in GNU and Clang modes that don't defer prototype instantiations, as
// well as in Microsoft C++ modes.
template<unsigned N> unsigned count(int (&array)[N]) {  // (1)
  return N;
}
template<typename T> struct C {
  static int x[];
  unsigned n() { return count(x); }  // (2)
};
template<typename T> int C<T>::x[] = { 1, 2 };
int main() {
  C<int> c;
  return c.n() != 2;
}
