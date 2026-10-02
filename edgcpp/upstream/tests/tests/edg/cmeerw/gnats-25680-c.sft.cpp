//type:fp
//options:--c11:--c11 --gcc:--c11 --clang

#ifdef __clang__
#define ATOMIC_SIZE 4
#else
#define ATOMIC_SIZE 3
#endif

struct C
{
  char arr[3];
};

typedef _Atomic(struct C) AC;

struct D
{
  AC ac;
  char c;
};

_Static_assert(sizeof(struct C) == 3, "sizeof C");
_Static_assert(sizeof(AC) == ATOMIC_SIZE, "sizeof AC");

_Static_assert(sizeof(struct D) == (ATOMIC_SIZE + 1 + 3) / 4 * 4, "sizeof D");

void foo(AC ac, struct C c, volatile struct C vc)
{
  ac = c;
  c = ac;
  ac = ac;
  vc = ac;
  ac = vc;
}
