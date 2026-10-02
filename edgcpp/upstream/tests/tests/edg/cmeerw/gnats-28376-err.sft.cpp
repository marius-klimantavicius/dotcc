//type:fn
//options:--c++11 --gn 150100

void f_atomic_compare_exchange(char *p1, char *p2, char v, int *p)
{
  __atomic_compare_exchange_n(p1, p2, v, false, p, 0);
  __atomic_compare_exchange_n(p1, p2, v, false, 0, p);
}

void f_atomic_compare_exchange(char *p1, char *p2, char *v)
{
  __atomic_compare_exchange_1(p1, p2, v, false, 0, 0);
  __atomic_compare_exchange_2(p1, p2, v, false, 0, 0);
  __atomic_compare_exchange_4(p1, p2, v, false, 0, 0);
  __atomic_compare_exchange_8(p1, p2, v, false, 0, 0);
}

void f_atomic_add_fetch(char *p, char v, int *q)
{
  __atomic_add_fetch(p, v, q);
}

void f_sync_bool_compare_and_swap(char *p, char *v1, char *v2)
{
  __sync_bool_compare_and_swap_1(p, v1, v2);
  __sync_bool_compare_and_swap_2(p, v1, v2);
  __sync_bool_compare_and_swap_4(p, v1, v2);
  __sync_bool_compare_and_swap_8(p, v1, v2);
}
