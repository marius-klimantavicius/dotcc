//type:fp
//options_all:--c++11
//remark:[4.11] Noexcept specifiers and member function declarations
// 2/23/16  [EDGcpfe/15632,EDGcpfe/16617,EDGcpfe/16810]
//
// Noexcept specifiers and member function declarations
//
// The front end now delays the parsing of the operand of a noexcept specifier for
// a member function declaration until the enclosing class has been completed
// (similar to the processing of default arguments).  This allows the noexcept
// specifier to depend on members of the class that have not been seen yet at the
// point where the noexcept specifier is encountered.
//
// The checking of exception specifications for overriding virtual functions has
// been delayed accordingly.  In Microsoft mode with microsoft_version == 1800
// this avoids spurious warnings on defaulted virtual destructors in some cases.
//
// In GNU C++ mode, parsing of the noexcept operand is not delayed; not even in
// template contexts.  The front end already attempted to emulate the GCC
// behavior in template contexts, but in some cases where a member function is
// defined outside the class (template) definition, this resulted in spurious
// errors.
//
// This is now fixed.
struct S {
  void f() noexcept(noexcept(g()));  // Previously an error; now okay.
  void g();
};

struct B { virtual ~B() = default; };
struct D: B {
  virtual ~D () = default;  // Previously triggered a spurious warning;
};                          // now silently accepted.

template<typename T> struct X { void f() noexcept(T()); };
template<typename T> void X<T>::f() noexcept(T()) {}
  // Previously triggered spurious errors ("T" is undefined) in
  // GNU C++11 mode.
