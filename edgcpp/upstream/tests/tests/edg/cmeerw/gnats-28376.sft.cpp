//type:fp
//options:--c++11 --gn 90100:--c++11 --gn 150100

namespace minimal
{
  void f(char *p, char *v) {
    __atomic_store_n(p, v, 0);
  }
}

void f_atomic_compare_exchange_n(char *p1, char *p2, char *v)
{
  __atomic_compare_exchange_n(p1, p2, v, false, 0, 0);
}

void f_atomic_exchange_n(char *p, char *v)
{
  __atomic_exchange_n(p, v, 0);
}

void f_atomic_store_n(char *p, char *v)
{
  __atomic_store_n(p, v, 0);
}

void f_atomic_add_fetch(char *p, char *v)
{
  __atomic_add_fetch(p, v, 0);
}

void f_sync_bool_compare_and_swap(char *p, char *v1, char *v2)
{
  __sync_bool_compare_and_swap(p, v1, v2);
}
