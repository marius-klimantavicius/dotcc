//type:fp
//options:--c++26
//options_all:-A -tused

void g();

template <typename T>
struct A {
  static void f() {
    g<T::TT>((A *)0);
  }
};

template <typename T> void g(void *);
template <auto> void g(void *);
template <template <typename> class> void g(void *);

struct Expr { enum { TT = 0 }; };
struct Type { using TT = int; };
struct Tmpl { template <typename> struct TT; };
void h() {
  A<Expr>::f(); // all accept
  A<Type>::f(); // EDG, MSVC accept; GCC, Clang rejects
  A<Tmpl>::f(); // EDG, MSVC accept; GCC, Clang rejects
}
