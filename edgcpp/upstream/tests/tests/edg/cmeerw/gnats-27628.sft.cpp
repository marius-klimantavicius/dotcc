//type:fp
//options:--c++14 --clang_version 190100

namespace minimal
{
  auto l = [] (auto t, auto *p) {
    __builtin_operator_new(t);
    __builtin_operator_delete(p);
  };
}

namespace operator_delete
{
  int i = (__builtin_operator_delete((void*)0), 0);

  template<typename T>
  int j = (__builtin_operator_delete((T *)0), 0);

  template<typename T>
  struct C
  {
    T t;
  };

  int jc = j<C<void>>;
}

namespace operator_new
{
  void *p = __builtin_operator_new(4);

  template<typename T>
  void *q = __builtin_operator_new(T(4));
}

namespace sync_and_atomic
{
  int v = 0;

  int i = (__sync_add_and_fetch(&v, 0), __atomic_load_n(&v, 0));

  template<typename T, T &r>
  int j = (__sync_add_and_fetch(&v, T(v)), __atomic_load_n(&v, T(v)),
           __sync_add_and_fetch(&r, T(v)), __atomic_load_n(&r, T(v)));
}

namespace substitution
{
  template<typename T>
  auto f1(T t) -> decltype(__builtin_operator_delete(t))
  { }

  template<typename T>
  auto f2(T t) -> decltype(::__builtin_operator_delete(t))
  { }

  template<typename T>
  auto f3(T t) -> decltype(::operator delete(t))
  { }

  template<typename T>
  struct C
  {
    T t;
  };

  void f(C<void> *p)
  {
    __builtin_operator_delete(p);
    ::__builtin_operator_delete(p);
    ::operator delete(p);
  }

  template void f1(C<void> *);
  template void f2(C<void> *);
  template void f3(C<void> *);
}
