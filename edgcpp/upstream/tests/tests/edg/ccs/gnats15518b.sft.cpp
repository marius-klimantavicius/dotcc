//type:cp
//options::--microsoft:--microsoft --no_ms_permissive
//options_all:--parse_templates

struct A;
template <class T>
struct B {
  typedef A U;
};
template<class T>
struct C : B<T> {
  C(typename B<T>::U& u) {
    u.f();
  }
};
