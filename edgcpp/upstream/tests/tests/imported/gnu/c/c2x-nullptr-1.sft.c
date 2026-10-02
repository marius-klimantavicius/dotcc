//type: rp
//options: --c23
# 0 "./c2x-nullptr-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c2x-nullptr-1.c"




# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 6 "./c2x-nullptr-1.c" 2


# 7 "./c2x-nullptr-1.c"
typedef __typeof__(nullptr) nullptr_t;

void f1 (nullptr_t) { }
void f2 (int *) { }
void f3 (_Bool) { }
nullptr_t cmp (void) { return nullptr; }




void
test1 (void)
{
  const nullptr_t nptr = nullptr;
  static nullptr_t static_nptr;
  int *p1 = nullptr;
  void *p2 = nullptr;
  float *p3 = nullptr;
  void (*p4)(int) = nullptr;
  int (*p5)[10] = nullptr;
  int *p6 = nptr;
  void *p7 = nptr;
  float *p8 = nptr;
  void (*p9)(int) = nptr;
  int (*p10)[10] = nptr;
  int *p11 = (int *) nullptr;
  int *p12 = (int *) nptr;
  int *p13 = (nullptr);
  int *p14 = _Generic(0, int : nullptr);
  if (nullptr || p1 || p2 || p3 || p4 || p5 || p6 || p7 || p8 || p9 || p10
      || p11 || p12 || p13 || p14)
    __builtin_abort ();

  _Bool b1 = nullptr;
  _Bool b2 = (_Bool) nullptr;
  _Bool b3 = nptr;
  _Bool b4 = (_Bool) nptr;
  _Bool b5 = _Generic(0, int : nullptr);
  if (b1 || b2 || b3 || b4 || b5 || (_Bool) nullptr || (_Bool) nptr)
   __builtin_abort ();

  __auto_type a1 = nullptr;
  __auto_type a2 = nptr;


  __typeof__(nullptr) x = nullptr;
  f1 (x);
  f1 (nullptr);
  f1 (_Generic(0, int : nullptr));
  f2 (x);
  f2 (nullptr);
  f3 (nullptr);

  const nullptr_t np1 = nullptr;
  const nullptr_t np2 = np1;
  (void) nullptr;
  (void) np1;
  (void) np2;
  (void) cmp ();
  (void)(nullptr_t) nullptr;

  const nullptr_t n = 0;
  (void) (nullptr_t) 0;

  f1 (0);
  f1 ((void *) 0);
  f1 (0L);
  nullptr_t n2;
  n2 = (void *) 0;
  n2 = 123 - 123;
  (void) n2;
}


void
test2 (int *p)
{


  const nullptr_t nptr = nullptr;
  int r = 0;


  r |= nullptr != nullptr;
  r |= cmp () != nullptr;
  r |= nullptr != cmp ();
  r |= !(nullptr == nullptr);
  r |= !(cmp () == nullptr);
  r |= !(nullptr == cmp ());
  r |= nptr != nptr;
  r |= cmp () != nptr;
  r |= nptr != cmp ();
  r |= !(nptr == nptr);
  r |= !(cmp () == nptr);
  r |= !(nptr == cmp ());


  r |= nullptr != (void *) 0;
  r |= _Generic(0, int : nullptr) != (void *) 0;
  r |= (nullptr) != (void *) 0;
  r |= !(nullptr == (void *) 0);
  r |= (void *) 0 != nullptr;
  r |= (void *) 0 != (nullptr);
  r |= !((void *) 0 == nullptr);
  r |= nullptr != 0;
  r |= _Generic(0, int : nullptr) != 0;
  r |= (nullptr) != 0;
  r |= 0 != nullptr;
  r |= 0 != (nullptr);
  r |= !(nullptr == 0);
  r |= !(0 == nullptr);
  r |= nullptr != 0u;
  r |= 0u != nullptr;
  r |= !(nullptr == 0u);
  r |= !(0u == nullptr);
  r |= nptr != (void *) 0;
  r |= !(nptr == (void *) 0);
  r |= (void *) 0 != nptr;
  r |= !((void *) 0 == nptr);
  r |= nptr != 0;
  r |= 0 != nptr;
  r |= !(nptr == 0);
  r |= !(0 == nptr);
  r |= nptr != 0u;
  r |= 0u != nptr;
  r |= !(nptr == 0u);
  r |= !(0u == nptr);
  r |= nptr != _Generic(0, int : nullptr);
  r |= _Generic(0, int : nullptr) != nptr;
  if (r)
    __builtin_abort ();


  (void) (p == nullptr);
  (void) (p != nullptr);
  (void) (nullptr == p);
  (void) (nullptr != p);
  (void) (p == (nullptr));
  (void) (p != (nullptr));
  (void) ((nullptr) == p);
  (void) ((nullptr) != p);
  (void) ((void *)nullptr == nullptr);
  (void) ((void *)nullptr != nullptr);
  (void) (nullptr == (void *)nullptr);
  (void) (nullptr != (void *)nullptr);
  (void) (p == _Generic(0, int : nullptr));
  (void) (p != _Generic(0, int : nullptr));
  (void) (_Generic(0, int : nullptr) == p);
  (void) (_Generic(0, int : nullptr) != p);




  (void) ((nullptr_t)nullptr == p);
  (void) ((nullptr_t)nullptr != p);
  (void) (p == (nullptr_t)nullptr);
  (void) (p != (nullptr_t)nullptr);
  (void) (cmp () == p);
  (void) (cmp () != p);
  (void) (p == cmp ());
  (void) (p != cmp ());

  (void) ((void *)nullptr == cmp ());
  (void) ((void *)nullptr != cmp ());
  (void) (cmp () == (void *)nullptr);
  (void) (cmp () != (void *)nullptr);
}


void
test3 (int *p, _Bool b)
{
  int x = nullptr ? 1 : 2;
  (void) x;
  const nullptr_t nptr = nullptr;


  __auto_type r1 = b ? nullptr : nullptr;
  __auto_type r2 = b ? nptr : nptr;


  __auto_type r3 = b ? p : nullptr;
  __auto_type r4 = b ? nullptr : p;
  __auto_type r5 = b ? nptr : p;
  __auto_type r6 = b ? p : nptr;
  __auto_type r7 = b ? 0 : p;
  __auto_type r8 = b ? p : 0;
  __auto_type r9 = b ? p : cmp ();
  __auto_type r10 = b ? cmp () : p;
  __auto_type r11 = b ? p : _Generic(0, int : nullptr);
  __auto_type r12 = b ? _Generic(0, int : nullptr) : p;
}

void test_arg1 (const nullptr_t, _Atomic nullptr_t, volatile nullptr_t) { }
void test_arg2 (_Atomic int *, const int *, volatile int *) { }
void test_arg3 (_Atomic _Bool, const _Bool, volatile _Bool) { }
nullptr_t retn (void) { return nullptr; }
_Atomic int *ai (void) { return nullptr; }
const int *ci (void) { return nullptr; }
volatile int *vi (void) { return nullptr; }
_Bool retb (void) { return nullptr; }


void
test4 (void)
{


  nullptr_t n1;
  const nullptr_t n2 = nullptr;
  _Atomic nullptr_t n3 = nullptr;
  volatile nullptr_t n4 = nullptr;
  _Atomic volatile nullptr_t n5 = nullptr;
  n1 = nullptr;
  n3 = nullptr;
  n4 = nullptr;
  n5 = nullptr;
  n5 = _Generic(0, int : nullptr);


  int *p1 = cmp ();
  _Atomic int *p2 = cmp ();
  const int *volatile p3 = cmp ();
  const int *const *const p4 = cmp ();
  double (*const p5)(void) = n1;
  p2 = _Generic(0, int : nullptr);
  p3 = nullptr;


  _Bool b1;
  b1 = cmp ();
  const _Bool b2 = nullptr;
  _Atomic _Bool b3;
  b3 = n1;
  (void) b1;
  (void) b3;
  (void) n3;
  (void) n4;
  (void) n5;
  (void) p2;
  (void) p3;

  test_arg1 (nullptr, nullptr, nullptr);
  test_arg2 (nullptr, nullptr, nullptr);
  test_arg3 (nullptr, nullptr, nullptr);
}


static void
test5 (int i, ...)
{
  (void) i;
  va_list ap;
  
# 260 "./c2x-nullptr-1.c" 3 4
 __builtin_va_start(
# 260 "./c2x-nullptr-1.c"
 ap
# 260 "./c2x-nullptr-1.c" 3 4
 , 0)
# 260 "./c2x-nullptr-1.c"
                 ;
  if (
# 261 "./c2x-nullptr-1.c" 3 4
     __builtin_va_arg(
# 261 "./c2x-nullptr-1.c"
     ap
# 261 "./c2x-nullptr-1.c" 3 4
     ,
# 261 "./c2x-nullptr-1.c"
     void *
# 261 "./c2x-nullptr-1.c" 3 4
     )
# 261 "./c2x-nullptr-1.c"
                        )
    __builtin_abort ();
}


void
test6 (void)
{
  _Static_assert (sizeof (nullptr) == sizeof (void *), "sizeof (nullptr)");
  _Static_assert (sizeof (nullptr_t) == sizeof (void *), "sizeof (nullptr_t)");
  _Static_assert (sizeof (nullptr) == sizeof (char *), "sizeof (nullptr)");
  _Static_assert (sizeof (nullptr_t) == sizeof (char *), "sizeof (nullptr_t)");
  _Static_assert (_Alignof (nullptr_t) == _Alignof (char *), "_Alignof (nullptr_t)");
  __typeof__(nullptr) t = nullptr;
  f1 (t);
  _Alignas (nullptr_t) char i1 = 'q';

  _Static_assert (_Generic (nullptr, nullptr_t: 1, default: 0) == 1, "_Generic");
  _Static_assert (_Generic (t, nullptr_t: 1, default: 0) == 1, "_Generic");
  _Static_assert (_Generic (cmp (), nullptr_t: 1, default: 0) == 1, "_Generic");
  _Static_assert (_Generic (0, nullptr_t: 1, int: 2, default: 0) == 2, "_Generic");
  _Static_assert (_Generic ((void *)0, nullptr_t: 1, void *: 2, default: 0) == 2, "_Generic");
  _Static_assert (_Generic (nullptr, nullptr_t: 1, void *: 2, default: 0) == 1, "_Generic");
}


void
test7 (void)
{
  if (nullptr)
    __builtin_abort ();
  if (1 && nullptr)
    __builtin_abort ();
  if (0 || nullptr)
    __builtin_abort ();
  if (nullptr && 1)
    __builtin_abort ();
  if (nullptr || 0)
    __builtin_abort ();
  if (!nullptr)
    {
    }
  else
    __builtin_abort ();
  while (nullptr)
    __builtin_abort ();
  int i = 0;
  do
    ++i;
  while (nullptr);
  if (i != 1)
    __builtin_abort ();
  for (;nullptr;)
    __builtin_abort ();
}

int
main (void)
{
  int i = 42;
  test1 ();
  test2 (&i);
  test3 (&i, 0);
  test4 ();
  test5 (42, nullptr);
  test6 ();
  test7 ();
}
