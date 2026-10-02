//type:fn
//options:--c++11:--c++11 --gn 70500:--c++11 --clang_version 160000:--c++20

// GCC/Clang sometimes ignores an additional "template<>" declaration, make
// sure we don't get completely confused.

namespace additional_empty_decl_scope
{
  template<class T> struct A {};

  template <>
  template <class T> struct A<T*> {
    void f(T);
    void g(T);
  };

  template <>
  template <class T> void A<T*>::f(T) {}

  template <class T> void A<T*>::g(T) {}
}

namespace additional_two_empty_decl_scopes
{
  template<class T> struct A {};

  template <>
  template <>
  template <class T> struct A<T*> {
    void f(T);
    void g(T);
  };

  template <>
  template <>
  template <class T> void A<T*>::f(T) {}

  template <class T> void A<T*>::g(T) {}
}

#if __cpp_concepts
namespace match_constrained_primary
{
  template<typename T> requires true
  class E
  { };

  template<typename T>
  class E<T>                    // error expected
  { };
}
#endif
