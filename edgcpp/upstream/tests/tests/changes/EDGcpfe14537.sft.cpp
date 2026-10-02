//type:fp
//options_all:--c++11
//remark:[4.10] Assertion failure during IA-64 mangling of constructor in unnamed class
// 12/15/14 [EDGcpfe/14537]
//
// Assertion failure during IA-64 mangling of constructor in unnamed class
//
// An assertion failure (in get_mangled_function_name_full) had resulted in
// configurations that use the IA-64 ABI on a case that inherits a constructor
// in an unnamed class.  Now fixed.
struct A {
  template <class T> A(const T&) {}
};
struct : A {
  using A::A;
} a(37);
