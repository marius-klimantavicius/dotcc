//type:fp
//remark:[4.12] GNU/Microsoft compatibility: Defaulted exception specifications
// 6/28/16  [EDGcpfe/17267]
//
// GNU/Microsoft compatibility: Defaulted exception specifications
//
// When explicitly defaulting a special member function, a declared exception
// specification on the defaulting declaration cannot conflict with the implicit
// exception specification.  In special members of template instantiations, GCC
// and MSVC mark the special member as deleted instead of diagnosing the conflict.
// The front end now does the same in Microsoft and GNU C++ modes.
//
// In this example, when T is E, the noexcept specification on X::X() conflicts
// with the generated one (because E::E() is not noexcept).  Ordinarily, that
// triggers an error, but in GNU and Microsoft mode it is now accepted and
// X<E>::X() is marked as deleted (as is S::S()).
struct E { E() {} };
template<typename T> struct X {
  X() noexcept = default;
  T t;
};
struct S { X<E> m; };
