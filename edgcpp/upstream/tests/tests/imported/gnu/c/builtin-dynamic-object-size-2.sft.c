//type: rp
//options: 
# 0 "./builtin-dynamic-object-size-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-dynamic-object-size-2.c"





# 1 "./builtin-object-size-2.c" 1




# 1 "./builtin-object-size-common.h" 1
typedef long unsigned int size_t;



  extern void exit (int);
  extern void *malloc (size_t);
  extern void free (void *);
  extern void *calloc (size_t, size_t);
  extern void *alloca (size_t);
  extern void *memcpy (void *, const void *, size_t);
  extern void *memset (void *, int, size_t);
  extern char *strcpy (char *, const char *);
  extern char *strdup (const char *);
  extern char *strndup (const char *, size_t);




unsigned nfails = 0;
# 6 "./builtin-object-size-2.c" 2

struct A
{
  char a[10];
  int b;
  char c[10];
} y, w[4];

extern char exta[];
extern char extb[30];
extern struct A extc[];
struct A zerol[0];

void
__attribute__ ((noinline))
test1 (void *q, int x)
{
  struct A a;
  void *p = &a.a[3], *r;
  char var[x + 10];
  struct A vara[x + 10];
  if (x < 0)
    r = &a.a[9];
  else
    r = &a.c[1];
  if (__builtin_dynamic_object_size (p, 1) != sizeof (a.a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 32); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&a.c[9], 1)
      != sizeof (a.c) - 9)
    do { __builtin_printf ("Failure at line: %d\n", 35); nfails++; } while (0);
  if (__builtin_dynamic_object_size (q, 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 37); nfails++; } while (0);

  if (x < 0
      ? __builtin_dynamic_object_size (r, 1) != sizeof (a.a) - 9
      : __builtin_dynamic_object_size (r, 1) != sizeof (a.c) - 1)
    do { __builtin_printf ("Failure at line: %d\n", 42); nfails++; } while (0);




  if (x < 6)
    r = &w[2].a[1];
  else
    r = &a.a[6];
  if (__builtin_dynamic_object_size (&y, 1) != sizeof (y))
    do { __builtin_printf ("Failure at line: %d\n", 52); nfails++; } while (0);
  if (__builtin_dynamic_object_size (w, 1) != sizeof (w))
    do { __builtin_printf ("Failure at line: %d\n", 54); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&y.b, 1) != sizeof (a.b))
    do { __builtin_printf ("Failure at line: %d\n", 56); nfails++; } while (0);

  if (x < 6
      ? __builtin_dynamic_object_size (r, 1) != sizeof (a.a) - 1
      : __builtin_dynamic_object_size (r, 1) != sizeof (a.a) - 6)
    do { __builtin_printf ("Failure at line: %d\n", 61); nfails++; } while (0);




  if (x < 20)
    r = malloc (30);
  else
    r = calloc (2, 16);

  if (__builtin_dynamic_object_size (r, 1) != (x < 20 ? 30 : 2 * 16))
    do { __builtin_printf ("Failure at line: %d\n", 72); nfails++; } while (0);
# 81 "./builtin-object-size-2.c"
  if (x < 20)
    r = malloc (30);
  else
    r = calloc (2, 14);

  if (__builtin_dynamic_object_size (r, 1) != (x < 20 ? 30 : 2 * 14))
    do { __builtin_printf ("Failure at line: %d\n", 87); nfails++; } while (0);




  if (x < 30)
    r = malloc (sizeof (a));
  else
    r = &a.a[3];

  if (__builtin_dynamic_object_size (r, 1) != (x < 30 ? sizeof (a) : sizeof (a) - 3))
    do { __builtin_printf ("Failure at line: %d\n", 98); nfails++; } while (0);




  r = memcpy (r, "a", 2);

  if (__builtin_dynamic_object_size (r, 1) != (x < 30 ? sizeof (a) : sizeof (a) - 3))
    do { __builtin_printf ("Failure at line: %d\n", 106); nfails++; } while (0);




  r = memcpy (r + 2, "b", 2) + 2;

  if (__builtin_dynamic_object_size (r, 0)
      != (x < 30 ? sizeof (a) - 4 : sizeof (a) - 7))
    do { __builtin_printf ("Failure at line: %d\n", 115); nfails++; } while (0);




  r = &a.a[4];
  r = memset (r, 'a', 2);
  if (__builtin_dynamic_object_size (r, 1) != sizeof (a.a) - 4)
    do { __builtin_printf ("Failure at line: %d\n", 123); nfails++; } while (0);
  r = memset (r + 2, 'b', 2) + 2;
  if (__builtin_dynamic_object_size (r, 1) != sizeof (a.a) - 8)
    do { __builtin_printf ("Failure at line: %d\n", 126); nfails++; } while (0);
  r = &a.a[1];
  r = strcpy (r, "ab");
  if (__builtin_dynamic_object_size (r, 1) != sizeof (a.a) - 1)
    do { __builtin_printf ("Failure at line: %d\n", 130); nfails++; } while (0);
  r = strcpy (r + 2, "cd") + 2;
  if (__builtin_dynamic_object_size (r, 1) != sizeof (a.a) - 5)
    do { __builtin_printf ("Failure at line: %d\n", 133); nfails++; } while (0);
  if (__builtin_dynamic_object_size (exta, 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 135); nfails++; } while (0);
  if (__builtin_dynamic_object_size (exta + 10, 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 137); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&exta[5], 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 139); nfails++; } while (0);
  if (__builtin_dynamic_object_size (extb, 1) != sizeof (extb))
    do { __builtin_printf ("Failure at line: %d\n", 141); nfails++; } while (0);
  if (__builtin_dynamic_object_size (extb + 10, 1) != sizeof (extb) - 10)
    do { __builtin_printf ("Failure at line: %d\n", 143); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&extb[5], 1) != sizeof (extb) - 5)
    do { __builtin_printf ("Failure at line: %d\n", 145); nfails++; } while (0);
  if (__builtin_dynamic_object_size (extc, 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 147); nfails++; } while (0);
  if (__builtin_dynamic_object_size (extc + 10, 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 149); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&extc[5], 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 151); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&extc->a, 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 153); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&(extc + 10)->b, 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 155); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&extc[5].c[3], 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 157); nfails++; } while (0);

  if (__builtin_dynamic_object_size (var, 1) != x + 10)
    do { __builtin_printf ("Failure at line: %d\n", 160); nfails++; } while (0);
  if (__builtin_dynamic_object_size (var + 10, 1) != x)
    do { __builtin_printf ("Failure at line: %d\n", 162); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&var[5], 1) != x + 5)
    do { __builtin_printf ("Failure at line: %d\n", 164); nfails++; } while (0);
  if (__builtin_dynamic_object_size (vara, 1) != (x + 10) * sizeof (struct A))
    do { __builtin_printf ("Failure at line: %d\n", 166); nfails++; } while (0);
  if (__builtin_dynamic_object_size (vara + 10, 1) != x * sizeof (struct A))
    do { __builtin_printf ("Failure at line: %d\n", 168); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&vara[5], 1) != (x + 5) * sizeof (struct A))
    do { __builtin_printf ("Failure at line: %d\n", 170); nfails++; } while (0);
# 185 "./builtin-object-size-2.c"
  if (__builtin_dynamic_object_size (&vara[0].a, 1) != sizeof (vara[0].a))
    do { __builtin_printf ("Failure at line: %d\n", 186); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&vara[10].a[0], 1) != sizeof (vara[0].a))
    do { __builtin_printf ("Failure at line: %d\n", 188); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&vara[5].a[4], 1) != sizeof (vara[0].a) - 4)
    do { __builtin_printf ("Failure at line: %d\n", 190); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&vara[5].b, 1) != sizeof (vara[0].b))
    do { __builtin_printf ("Failure at line: %d\n", 192); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&vara[7].c[7], 1) != sizeof (vara[0].c) - 7)
    do { __builtin_printf ("Failure at line: %d\n", 194); nfails++; } while (0);
  if (__builtin_dynamic_object_size (zerol, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 196); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&zerol, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 198); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&zerol[0], 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 200); nfails++; } while (0);
  if (__builtin_dynamic_object_size (zerol[0].a, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 202); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&zerol[0].a[0], 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 204); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&zerol[0].b, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 206); nfails++; } while (0);
  if (__builtin_dynamic_object_size ("abcdefg", 1) != sizeof ("abcdefg"))
    do { __builtin_printf ("Failure at line: %d\n", 208); nfails++; } while (0);
  if (__builtin_dynamic_object_size ("abcd\0efg", 1) != sizeof ("abcd\0efg"))
    do { __builtin_printf ("Failure at line: %d\n", 210); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&"abcd\0efg", 1) != sizeof ("abcd\0efg"))
    do { __builtin_printf ("Failure at line: %d\n", 212); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&"abcd\0efg"[0], 1) != sizeof ("abcd\0efg"))
    do { __builtin_printf ("Failure at line: %d\n", 214); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&"abcd\0efg"[4], 1) != sizeof ("abcd\0efg") - 4)
    do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0);
  if (__builtin_dynamic_object_size ("abcd\0efg" + 5, 1) != sizeof ("abcd\0efg") - 5)
    do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0);
  if (__builtin_dynamic_object_size (L"abcdefg", 1) != sizeof (L"abcdefg"))
    do { __builtin_printf ("Failure at line: %d\n", 220); nfails++; } while (0);
  r = (char *) L"abcd\0efg";
  if (__builtin_dynamic_object_size (r + 2, 1) != sizeof (L"abcd\0efg") - 2)
    do { __builtin_printf ("Failure at line: %d\n", 223); nfails++; } while (0);
}

size_t l1 = 1;

void
__attribute__ ((noinline))
test2 (void)
{
  struct B { char buf1[10]; char buf2[10]; } a;
  char *r, buf3[20];
  int i;

  size_t dyn_res;


  if (sizeof (a) != 20)
    return;

  r = buf3;
  for (i = 0; i < 4; ++i)
    {
      if (i == l1 - 1)
 r = &a.buf1[1];
      else if (i == l1)
 r = &a.buf2[7];
      else if (i == l1 + 1)
 r = &buf3[5];
      else if (i == l1 + 2)
 r = &a.buf1[9];
    }

  dyn_res = sizeof (buf3);

  for (i = 0; i < 4; ++i)
    {
      if (i == l1 - 1)
 dyn_res = sizeof (a.buf1) - 1;
      else if (i == l1)
 dyn_res = sizeof (a.buf2) - 7;
      else if (i == l1 + 1)
 dyn_res = sizeof (buf3) - 5;
      else if (i == l1 + 2)
 dyn_res = sizeof (a.buf1) - 9;
    }
  if (__builtin_dynamic_object_size (r, 1) != dyn_res)
    do { __builtin_printf ("Failure at line: %d\n", 269); nfails++; } while (0);




  r = &buf3[20];
  for (i = 0; i < 4; ++i)
    {
      if (i == l1 - 1)
 r = &a.buf1[7];
      else if (i == l1)
 r = &a.buf2[7];
      else if (i == l1 + 1)
 r = &buf3[5];
      else if (i == l1 + 2)
 r = &a.buf1[9];
    }

  dyn_res = sizeof (buf3) - 20;

  for (i = 0; i < 4; ++i)
    {
      if (i == l1 - 1)
 dyn_res = sizeof (a.buf1) - 7;
      else if (i == l1)
 dyn_res = sizeof (a.buf2) - 7;
      else if (i == l1 + 1)
 dyn_res = sizeof (buf3) - 5;
      else if (i == l1 + 2)
 dyn_res = sizeof (a.buf1) - 9;
    }
  if (__builtin_dynamic_object_size (r, 1) != dyn_res)
    do { __builtin_printf ("Failure at line: %d\n", 301); nfails++; } while (0);




  r += 8;

  if (dyn_res >= 8)
    {
      dyn_res -= 8;
      if (__builtin_dynamic_object_size (r, 1) != dyn_res)
 do { __builtin_printf ("Failure at line: %d\n", 312); nfails++; } while (0);

      if (dyn_res >= 6)
 {
   if (__builtin_dynamic_object_size (r + 6, 1) != dyn_res - 6)
     do { __builtin_printf ("Failure at line: %d\n", 317); nfails++; } while (0);
 }
      else if (__builtin_dynamic_object_size (r + 6, 1) != 0)
 do { __builtin_printf ("Failure at line: %d\n", 320); nfails++; } while (0);
    }
  else if (__builtin_dynamic_object_size (r, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 323); nfails++; } while (0);






}

void
__attribute__ ((noinline))
test3 (void)
{
  char buf4[10];
  struct B { struct A a[2]; struct A b; char c[4]; char d; double e;
      _Complex double f; } x;
  double y;
  _Complex double z;
  double *dp;

  if (__builtin_dynamic_object_size (buf4, 1) != sizeof (buf4))
    do { __builtin_printf ("Failure at line: %d\n", 344); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&buf4, 1) != sizeof (buf4))
    do { __builtin_printf ("Failure at line: %d\n", 346); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&buf4[0], 1) != sizeof (buf4))
    do { __builtin_printf ("Failure at line: %d\n", 348); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&buf4[1], 1) != sizeof (buf4) - 1)
    do { __builtin_printf ("Failure at line: %d\n", 350); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x, 1) != sizeof (x))
    do { __builtin_printf ("Failure at line: %d\n", 352); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a, 1) != sizeof (x.a))
    do { __builtin_printf ("Failure at line: %d\n", 354); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[0], 1) != sizeof (x.a))
    do { __builtin_printf ("Failure at line: %d\n", 356); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[0].a, 1) != sizeof (x.a[0].a))
    do { __builtin_printf ("Failure at line: %d\n", 358); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[0].a[0], 1) != sizeof (x.a[0].a))
    do { __builtin_printf ("Failure at line: %d\n", 360); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[0].a[3], 1) != sizeof (x.a[0].a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 362); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[0].b, 1) != sizeof (x.a[0].b))
    do { __builtin_printf ("Failure at line: %d\n", 364); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[1].c, 1) != sizeof (x.a[1].c))
    do { __builtin_printf ("Failure at line: %d\n", 366); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[1].c[0], 1) != sizeof (x.a[1].c))
    do { __builtin_printf ("Failure at line: %d\n", 368); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[1].c[3], 1) != sizeof (x.a[1].c) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 370); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b, 1) != sizeof (x.b))
    do { __builtin_printf ("Failure at line: %d\n", 372); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.a, 1) != sizeof (x.b.a))
    do { __builtin_printf ("Failure at line: %d\n", 374); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.a[0], 1) != sizeof (x.b.a))
    do { __builtin_printf ("Failure at line: %d\n", 376); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.a[3], 1) != sizeof (x.b.a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 378); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.b, 1) != sizeof (x.b.b))
    do { __builtin_printf ("Failure at line: %d\n", 380); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.c, 1) != sizeof (x.b.c))
    do { __builtin_printf ("Failure at line: %d\n", 382); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.c[0], 1) != sizeof (x.b.c))
    do { __builtin_printf ("Failure at line: %d\n", 384); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.c[3], 1) != sizeof (x.b.c) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 386); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.c, 1) != sizeof (x.c))
    do { __builtin_printf ("Failure at line: %d\n", 388); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.c[0], 1) != sizeof (x.c))
    do { __builtin_printf ("Failure at line: %d\n", 390); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.c[1], 1) != sizeof (x.c) - 1)
    do { __builtin_printf ("Failure at line: %d\n", 392); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.d, 1) != sizeof (x.d))
    do { __builtin_printf ("Failure at line: %d\n", 394); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.e, 1) != sizeof (x.e))
    do { __builtin_printf ("Failure at line: %d\n", 396); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.f, 1) != sizeof (x.f))
    do { __builtin_printf ("Failure at line: %d\n", 398); nfails++; } while (0);
  dp = &__real__ x.f;
  if (__builtin_dynamic_object_size (dp, 1) != sizeof (x.f) / 2)
    do { __builtin_printf ("Failure at line: %d\n", 401); nfails++; } while (0);
  dp = &__imag__ x.f;
  if (__builtin_dynamic_object_size (dp, 1) != sizeof (x.f) / 2)
    do { __builtin_printf ("Failure at line: %d\n", 404); nfails++; } while (0);
  dp = &y;
  if (__builtin_dynamic_object_size (dp, 1) != sizeof (y))
    do { __builtin_printf ("Failure at line: %d\n", 407); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&z, 1) != sizeof (z))
      do { __builtin_printf ("Failure at line: %d\n", 409); nfails++; } while (0);
  dp = &__real__ z;
  if (__builtin_dynamic_object_size (dp, 1) != sizeof (z) / 2)
    do { __builtin_printf ("Failure at line: %d\n", 412); nfails++; } while (0);
  dp = &__imag__ z;
  if (__builtin_dynamic_object_size (dp, 1) != sizeof (z) / 2)
    do { __builtin_printf ("Failure at line: %d\n", 415); nfails++; } while (0);
}

struct S { unsigned int a; };

char *
__attribute__ ((noinline))
test4 (char *x, int y)
{
  register int i;
  struct A *p;

  for (i = 0; i < y; i++)
    {
      p = (struct A *) x;
      x = (char *) &p[1];
      if (__builtin_dynamic_object_size (p, 1) != (size_t) -1)
 do { __builtin_printf ("Failure at line: %d\n", 432); nfails++; } while (0);
    }
  return x;
}

void
__attribute__ ((noinline))
test5 (size_t x)
{
  struct T { char buf[64]; char buf2[64]; } t;
  char *p = &t.buf[8];
  size_t i;

  for (i = 0; i < x; ++i)
    p = p + 4;

  if (__builtin_dynamic_object_size (p, 1) != sizeof (t.buf) - 8 - 4 * x)
    do { __builtin_printf ("Failure at line: %d\n", 449); nfails++; } while (0);




  memset (p, ' ', sizeof (t.buf) - 8 - 4 * 4);
}

void
__attribute__ ((noinline))
test6 (void)
{
  char buf[64];
  struct T { char buf[64]; char buf2[64]; } t;
  char *p = &buf[64], *q = &t.buf[64];

  if (__builtin_dynamic_object_size (p + 64, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 466); nfails++; } while (0);
  if (__builtin_dynamic_object_size (q + 0, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 468); nfails++; } while (0);
  if (__builtin_dynamic_object_size (q + 64, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 470); nfails++; } while (0);
}

void
__attribute__ ((noinline))
test7 (void)
{
  struct T { char buf[10]; char buf2[10]; } t;
  char *p = &t.buf2[-4];
  char *q = &t.buf2[0];
  if (__builtin_dynamic_object_size (p, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 481); nfails++; } while (0);
  if (__builtin_dynamic_object_size (q, 1) != sizeof (t.buf2))
    do { __builtin_printf ("Failure at line: %d\n", 483); nfails++; } while (0);
  q = &t.buf[10];
  if (__builtin_dynamic_object_size (q, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 486); nfails++; } while (0);
  q = &t.buf[11];
  if (__builtin_dynamic_object_size (q, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 489); nfails++; } while (0);
  p = &t.buf[-4];
  if (__builtin_dynamic_object_size (p, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 492); nfails++; } while (0);
}

void
__attribute__ ((noinline))
test8 (unsigned cond)
{
  char *buf2 = malloc (10);
  char *p;

  if (cond)
    p = &buf2[8];
  else
    p = &buf2[4];


  if (__builtin_dynamic_object_size (&p[-4], 1) != (cond ? 6 : 10))
    do { __builtin_printf ("Failure at line: %d\n", 509); nfails++; } while (0);





  for (unsigned i = cond; i > 0; i--)
    p--;


  if (__builtin_dynamic_object_size (p, 1) != ((cond ? 2 : 6) + cond))
    do { __builtin_printf ("Failure at line: %d\n", 520); nfails++; } while (0);





  p = &y.c[8];
  for (unsigned i = cond; i > 0; i--)
    p--;


  if (__builtin_dynamic_object_size (p, 1) != sizeof (y.c) - 8 + cond)
    do { __builtin_printf ("Failure at line: %d\n", 532); nfails++; } while (0);




}



size_t
__attribute__ ((noinline))
test9 (void)
{
  const char *ptr = "abcdefghijklmnopqrstuvwxyz";
  char *res = strndup (ptr, 21);
  if (__builtin_dynamic_object_size (res, 1) != 22)
    do { __builtin_printf ("Failure at line: %d\n", 548); nfails++; } while (0);

  free (res);

  res = strndup (ptr, 32);
  if (__builtin_dynamic_object_size (res, 1) != 27)
    do { __builtin_printf ("Failure at line: %d\n", 554); nfails++; } while (0);

  free (res);

  res = strdup (ptr);
  if (__builtin_dynamic_object_size (res, 1) != 27)
    do { __builtin_printf ("Failure at line: %d\n", 560); nfails++; } while (0);

  free (res);

  char *ptr2 = malloc (64);
  strcpy (ptr2, ptr);

  res = strndup (ptr2, 21);
  if (__builtin_dynamic_object_size (res, 1) != 22)
    do { __builtin_printf ("Failure at line: %d\n", 569); nfails++; } while (0);

  free (res);

  res = strndup (ptr2, 32);
  if (__builtin_dynamic_object_size (res, 1) != 33)
    do { __builtin_printf ("Failure at line: %d\n", 575); nfails++; } while (0);

  free (res);

  res = strndup (ptr2, 128);
  if (__builtin_dynamic_object_size (res, 1) != 64)
    do { __builtin_printf ("Failure at line: %d\n", 581); nfails++; } while (0);

  free (res);

  res = strdup (ptr2);

  if (__builtin_dynamic_object_size (res, 1) != 27)



    do { __builtin_printf ("Failure at line: %d\n", 591); nfails++; } while (0);

  free (res);
  free (ptr2);

  ptr = "abcd\0efghijklmnopqrstuvwxyz";
  res = strdup (ptr);
  if (__builtin_dynamic_object_size (res, 1) != 5)
    do { __builtin_printf ("Failure at line: %d\n", 599); nfails++; } while (0);
  free (res);

  res = strndup (ptr, 24);
  if (__builtin_dynamic_object_size (res, 1) != 5)
    do { __builtin_printf ("Failure at line: %d\n", 604); nfails++; } while (0);
  free (res);

  res = strndup (ptr, 2);
  if (__builtin_dynamic_object_size (res, 1) != 3)
    do { __builtin_printf ("Failure at line: %d\n", 609); nfails++; } while (0);
  free (res);

  res = strdup (&ptr[4]);
  if (__builtin_dynamic_object_size (res, 1) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 614); nfails++; } while (0);
  free (res);

  res = strndup (&ptr[4], 4);
  if (__builtin_dynamic_object_size (res, 1) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 619); nfails++; } while (0);
  free (res);

  res = strndup (&ptr[4], 1);
  if (__builtin_dynamic_object_size (res, 1) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 624); nfails++; } while (0);
  free (res);
}


int
main (void)
{
  struct S s[10];
  __asm ("" : "=r" (l1) : "0" (l1));
  test1 (main, 6);
  test2 ();
  test3 ();
  test4 ((char *) s, 10);
  test5 (4);
  test6 ();
  test7 ();
  test8 (1);

  test9 ();

  do { if (nfails > 0) __builtin_abort (); return 0; } while (0);
}
# 7 "./builtin-dynamic-object-size-2.c" 2
