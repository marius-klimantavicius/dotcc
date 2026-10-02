//type:fp
//options_all:--microsoft
//remark:[4.8] Spurious error on incomplete base class in class template in Microsoft mode
// 6/3/13   [EDGcpfe/13617]
//
// Spurious error on incomplete base class in class template in Microsoft mode
//
// The front end approximates the behavior of Microsoft compilers with respect to
// dependent base classes by performing a "nonreal instantiation" of such base
// classes (i.e., an instantiation where the template parameters are treated as
// generic types).  However, that process could lead to incomplete base classes of
// non-dependent types, which previously triggered a spurious error.
//
// This is now fixed: In Microsoft mode incomplete bases classes are now permitted
// in nonreal instantiations of class templates.
struct I;  // Incomplete class
template<typename T, typename U> struct S: U {};
template<typename T> struct X: S<T, I> {};
  // S<T, I> is a dependent class for which the front end performs a
  // nonreal instantiation.  However, S<T, I> has a base class of type I,
  // which is nondependent and previously triggered an error.
