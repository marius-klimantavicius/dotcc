template<typename>
void f()
{
  _Atomic(bool) atom;

  __c11_atomic_exchange(&atom, 0, 0);
};

struct B
{
  _Atomic(bool) atom;

  void f() volatile {
    __c11_atomic_exchange(&atom, 0, 0);
  }

  void g() {
    __c11_atomic_exchange(&atom, 0, 0);
  }
};
