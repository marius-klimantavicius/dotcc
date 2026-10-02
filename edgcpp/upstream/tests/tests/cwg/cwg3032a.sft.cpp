//type:fp
//options:--c++23
//options_all:-A -tused

void g();

template <typename T>
struct A {
  static void f() {
    g<A>((A *)0);     // all accept
    g<A>((A *)0, 0);  // Clang, GCC, EDG accept; MSVC rejects
  }
};

template <typename T> void g(void *);
template <template <typename> class> void g(void *, int);

void h() { A<int>::f(); }

//cwg: 3032
//title: Template argument disambiguation
//meeting: Kona 11/25
//edg_status: Passes
