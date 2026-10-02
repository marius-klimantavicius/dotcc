//type: rp
//options: 
# 0 "./builtin-dynamic-object-size-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-dynamic-object-size-4.c"





# 1 "./builtin-object-size-4.c" 1




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
# 6 "./builtin-object-size-4.c" 2

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
  if (__builtin_dynamic_object_size (p, 3) != sizeof (a.a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 32); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&a.c[9], 3)
      != sizeof (a.c) - 9)
    do { __builtin_printf ("Failure at line: %d\n", 35); nfails++; } while (0);
  if (__builtin_dynamic_object_size (q, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 37); nfails++; } while (0);

  if (__builtin_dynamic_object_size (r, 3)
      != (x < 0 ? sizeof (a.a) - 9 : sizeof (a.c) - 1))



    do { __builtin_printf ("Failure at line: %d\n", 44); nfails++; } while (0);
  if (x < 6)
    r = &w[2].a[1];
  else
    r = &a.a[6];
  if (__builtin_dynamic_object_size (&y, 3) != sizeof (y))
    do { __builtin_printf ("Failure at line: %d\n", 50); nfails++; } while (0);
  if (__builtin_dynamic_object_size (w, 3) != sizeof (w))
    do { __builtin_printf ("Failure at line: %d\n", 52); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&y.b, 3) != sizeof (a.b))
    do { __builtin_printf ("Failure at line: %d\n", 54); nfails++; } while (0);

  if (__builtin_dynamic_object_size (r, 3)
      != (x < 6 ? sizeof (w[2].a) - 1 : sizeof (a.a) - 6))



    do { __builtin_printf ("Failure at line: %d\n", 61); nfails++; } while (0);
  if (x < 20)
    r = malloc (30);
  else
    r = calloc (2, 16);

  if (__builtin_dynamic_object_size (r, 3) != (x < 20 ? 30 : 2 * 16))



    do { __builtin_printf ("Failure at line: %d\n", 71); nfails++; } while (0);
  if (x < 20)
    r = malloc (30);
  else
    r = calloc (2, 14);

  if (__builtin_dynamic_object_size (r, 3) != (x < 20 ? 30 : 2 * 14))



    do { __builtin_printf ("Failure at line: %d\n", 81); nfails++; } while (0);
  if (x < 30)
    r = malloc (sizeof (a));
  else
    r = &a.a[3];

  size_t objsz = x < 30 ? sizeof (a) : sizeof (a.a) - 3;
  if (__builtin_dynamic_object_size (r, 3) != objsz)



    do { __builtin_printf ("Failure at line: %d\n", 92); nfails++; } while (0);
  r = memcpy (r, "a", 2);

  if (__builtin_dynamic_object_size (r, 3) != objsz)



    do { __builtin_printf ("Failure at line: %d\n", 99); nfails++; } while (0);
  r = memcpy (r + 2, "b", 2) + 2;

  if (__builtin_dynamic_object_size (r, 3) != objsz - 4)



    do { __builtin_printf ("Failure at line: %d\n", 106); nfails++; } while (0);
  r = &a.a[4];
  r = memset (r, 'a', 2);
  if (__builtin_dynamic_object_size (r, 3) != sizeof (a.a) - 4)
    do { __builtin_printf ("Failure at line: %d\n", 110); nfails++; } while (0);
  r = memset (r + 2, 'b', 2) + 2;
  if (__builtin_dynamic_object_size (r, 3) != sizeof (a.a) - 8)
    do { __builtin_printf ("Failure at line: %d\n", 113); nfails++; } while (0);
  r = &a.a[1];
  r = strcpy (r, "ab");
  if (__builtin_dynamic_object_size (r, 3) != sizeof (a.a) - 1)
    do { __builtin_printf ("Failure at line: %d\n", 117); nfails++; } while (0);
  r = strcpy (r + 2, "cd") + 2;
  if (__builtin_dynamic_object_size (r, 3) != sizeof (a.a) - 5)
    do { __builtin_printf ("Failure at line: %d\n", 120); nfails++; } while (0);
  if (__builtin_dynamic_object_size (exta, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 122); nfails++; } while (0);
  if (__builtin_dynamic_object_size (exta + 10, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 124); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&exta[5], 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 126); nfails++; } while (0);
  if (__builtin_dynamic_object_size (extb, 3) != sizeof (extb))
    do { __builtin_printf ("Failure at line: %d\n", 128); nfails++; } while (0);
  if (__builtin_dynamic_object_size (extb + 10, 3) != sizeof (extb) - 10)
    do { __builtin_printf ("Failure at line: %d\n", 130); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&extb[5], 3) != sizeof (extb) - 5)
    do { __builtin_printf ("Failure at line: %d\n", 132); nfails++; } while (0);
  if (__builtin_dynamic_object_size (extc, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 134); nfails++; } while (0);
  if (__builtin_dynamic_object_size (extc + 10, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 136); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&extc[5], 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 138); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&extc->a, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 140); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&(extc + 10)->b, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&extc[5].c[3], 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0);

  if (__builtin_dynamic_object_size (var, 3) != x + 10)
    do { __builtin_printf ("Failure at line: %d\n", 147); nfails++; } while (0);
  if (__builtin_dynamic_object_size (var + 10, 3) != x)
    do { __builtin_printf ("Failure at line: %d\n", 149); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&var[5], 3) != x + 5)
    do { __builtin_printf ("Failure at line: %d\n", 151); nfails++; } while (0);
  if (__builtin_dynamic_object_size (vara, 3) != (x + 10) * sizeof (struct A))
    do { __builtin_printf ("Failure at line: %d\n", 153); nfails++; } while (0);
  if (__builtin_dynamic_object_size (vara + 10, 3) != x * sizeof (struct A))
    do { __builtin_printf ("Failure at line: %d\n", 155); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&vara[5], 3) != (x + 5) * sizeof (struct A))
    do { __builtin_printf ("Failure at line: %d\n", 157); nfails++; } while (0);
# 172 "./builtin-object-size-4.c"
  if (__builtin_dynamic_object_size (&vara[0].a, 3) != sizeof (vara[0].a))
    do { __builtin_printf ("Failure at line: %d\n", 173); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&vara[10].a[0], 3) != sizeof (vara[0].a))
    do { __builtin_printf ("Failure at line: %d\n", 175); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&vara[5].a[4], 3) != sizeof (vara[0].a) - 4)
    do { __builtin_printf ("Failure at line: %d\n", 177); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&vara[5].b, 3) != sizeof (vara[0].b))
    do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&vara[7].c[7], 3) != sizeof (vara[0].c) - 7)
    do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0);
  if (__builtin_dynamic_object_size (zerol, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 183); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&zerol, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 185); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&zerol[0], 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 187); nfails++; } while (0);
  if (__builtin_dynamic_object_size (zerol[0].a, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 189); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&zerol[0].a[0], 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 191); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&zerol[0].b, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 193); nfails++; } while (0);
  if (__builtin_dynamic_object_size ("abcdefg", 3) != sizeof ("abcdefg"))
    do { __builtin_printf ("Failure at line: %d\n", 195); nfails++; } while (0);
  if (__builtin_dynamic_object_size ("abcd\0efg", 3) != sizeof ("abcd\0efg"))
    do { __builtin_printf ("Failure at line: %d\n", 197); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&"abcd\0efg", 3) != sizeof ("abcd\0efg"))
    do { __builtin_printf ("Failure at line: %d\n", 199); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&"abcd\0efg"[0], 3) != sizeof ("abcd\0efg"))
    do { __builtin_printf ("Failure at line: %d\n", 201); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&"abcd\0efg"[4], 3) != sizeof ("abcd\0efg") - 4)
    do { __builtin_printf ("Failure at line: %d\n", 203); nfails++; } while (0);
  if (__builtin_dynamic_object_size ("abcd\0efg" + 5, 3) != sizeof ("abcd\0efg") - 5)
    do { __builtin_printf ("Failure at line: %d\n", 205); nfails++; } while (0);
  if (__builtin_dynamic_object_size (L"abcdefg", 3) != sizeof (L"abcdefg"))
    do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0);
  r = (char *) L"abcd\0efg";
  if (__builtin_dynamic_object_size (r + 2, 3) != sizeof (L"abcd\0efg") - 2)
    do { __builtin_printf ("Failure at line: %d\n", 210); nfails++; } while (0);


  asm volatile ("" : : "g" (&a));
}

size_t l1 = 1;

void
__attribute__ ((noinline))
test2 (void)
{
  struct B { char buf1[10]; char buf2[10]; } a;
  char *r, buf3[20];
  int i;

  size_t dyn_res = 0;


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
  if (__builtin_dynamic_object_size (r, 3) != sizeof (a.buf1) - 9)
    do { __builtin_printf ("Failure at line: %d\n", 245); nfails++; } while (0);
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
  if (__builtin_dynamic_object_size (r, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 259); nfails++; } while (0);
  r = &buf3[1];
  for (i = 0; i < 4; ++i)
    {
      if (i == l1 - 1)
 r = &a.buf1[6];
      else if (i == l1)
 r = &a.buf2[4];
      else if (i == l1 + 1)
 r = &buf3[5];
      else if (i == l1 + 2)
 r = &a.buf1[2];
    }

  dyn_res = sizeof (buf3) - 1;

  for (i = 0; i < 4; ++i)
    {
      if (i == l1 - 1)
 dyn_res = sizeof (a.buf1) - 6;
      else if (i == l1)
 dyn_res = sizeof (a.buf2) - 4;
      else if (i == l1 + 1)
 dyn_res = sizeof (buf3) - 5;
      else if (i == l1 + 2)
 dyn_res = sizeof (a.buf1) - 2;
    }
  if (__builtin_dynamic_object_size (r, 3) != dyn_res)
    do { __builtin_printf ("Failure at line: %d\n", 287); nfails++; } while (0);




  r += 2;

  if (__builtin_dynamic_object_size (r, 3) != dyn_res - 2)
    do { __builtin_printf ("Failure at line: %d\n", 295); nfails++; } while (0);
  if (__builtin_dynamic_object_size (r + 1, 3) != dyn_res - 3)
    do { __builtin_printf ("Failure at line: %d\n", 297); nfails++; } while (0);






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

  if (__builtin_dynamic_object_size (buf4, 3) != sizeof (buf4))
    do { __builtin_printf ("Failure at line: %d\n", 318); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&buf4, 3) != sizeof (buf4))
    do { __builtin_printf ("Failure at line: %d\n", 320); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&buf4[0], 3) != sizeof (buf4))
    do { __builtin_printf ("Failure at line: %d\n", 322); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&buf4[1], 3) != sizeof (buf4) - 1)
    do { __builtin_printf ("Failure at line: %d\n", 324); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x, 3) != sizeof (x))
    do { __builtin_printf ("Failure at line: %d\n", 326); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a, 3) != sizeof (x.a))
    do { __builtin_printf ("Failure at line: %d\n", 328); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[0], 3) != sizeof (x.a))
    do { __builtin_printf ("Failure at line: %d\n", 330); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[0].a, 3) != sizeof (x.a[0].a))
    do { __builtin_printf ("Failure at line: %d\n", 332); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[0].a[0], 3) != sizeof (x.a[0].a))
    do { __builtin_printf ("Failure at line: %d\n", 334); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[0].a[3], 3) != sizeof (x.a[0].a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 336); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[0].b, 3) != sizeof (x.a[0].b))
    do { __builtin_printf ("Failure at line: %d\n", 338); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[1].c, 3) != sizeof (x.a[1].c))
    do { __builtin_printf ("Failure at line: %d\n", 340); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[1].c[0], 3) != sizeof (x.a[1].c))
    do { __builtin_printf ("Failure at line: %d\n", 342); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.a[1].c[3], 3) != sizeof (x.a[1].c) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 344); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b, 3) != sizeof (x.b))
    do { __builtin_printf ("Failure at line: %d\n", 346); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.a, 3) != sizeof (x.b.a))
    do { __builtin_printf ("Failure at line: %d\n", 348); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.a[0], 3) != sizeof (x.b.a))
    do { __builtin_printf ("Failure at line: %d\n", 350); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.a[3], 3) != sizeof (x.b.a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 352); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.b, 3) != sizeof (x.b.b))
    do { __builtin_printf ("Failure at line: %d\n", 354); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.c, 3) != sizeof (x.b.c))
    do { __builtin_printf ("Failure at line: %d\n", 356); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.c[0], 3) != sizeof (x.b.c))
    do { __builtin_printf ("Failure at line: %d\n", 358); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.b.c[3], 3) != sizeof (x.b.c) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 360); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.c, 3) != sizeof (x.c))
    do { __builtin_printf ("Failure at line: %d\n", 362); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.c[0], 3) != sizeof (x.c))
    do { __builtin_printf ("Failure at line: %d\n", 364); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.c[1], 3) != sizeof (x.c) - 1)
    do { __builtin_printf ("Failure at line: %d\n", 366); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.d, 3) != sizeof (x.d))
    do { __builtin_printf ("Failure at line: %d\n", 368); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.e, 3) != sizeof (x.e))
    do { __builtin_printf ("Failure at line: %d\n", 370); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&x.f, 3) != sizeof (x.f))
    do { __builtin_printf ("Failure at line: %d\n", 372); nfails++; } while (0);
  dp = &__real__ x.f;
  if (__builtin_dynamic_object_size (dp, 3) != sizeof (x.f) / 2)
    do { __builtin_printf ("Failure at line: %d\n", 375); nfails++; } while (0);
  dp = &__imag__ x.f;
  if (__builtin_dynamic_object_size (dp, 3) != sizeof (x.f) / 2)
    do { __builtin_printf ("Failure at line: %d\n", 378); nfails++; } while (0);
  dp = &y;
  if (__builtin_dynamic_object_size (dp, 3) != sizeof (y))
    do { __builtin_printf ("Failure at line: %d\n", 381); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&z, 3) != sizeof (z))
      do { __builtin_printf ("Failure at line: %d\n", 383); nfails++; } while (0);
  dp = &__real__ z;
  if (__builtin_dynamic_object_size (dp, 3) != sizeof (z) / 2)
    do { __builtin_printf ("Failure at line: %d\n", 386); nfails++; } while (0);
  dp = &__imag__ z;
  if (__builtin_dynamic_object_size (dp, 3) != sizeof (z) / 2)
    do { __builtin_printf ("Failure at line: %d\n", 389); nfails++; } while (0);
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
      if (__builtin_dynamic_object_size (p, 3) != 0)
 do { __builtin_printf ("Failure at line: %d\n", 406); nfails++; } while (0);
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

  if (__builtin_dynamic_object_size (p, 3) != sizeof (t.buf) - 8 - 4 * x)



    do { __builtin_printf ("Failure at line: %d\n", 426); nfails++; } while (0);
  memset (p, ' ', sizeof (t.buf) - 8 - 4 * 4);
}

void
__attribute__ ((noinline))
test6 (void)
{
  char buf[64];
  struct T { char buf[64]; char buf2[64]; } t;
  char *p = &buf[64], *q = &t.buf[64];

  if (__builtin_dynamic_object_size (p + 64, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 439); nfails++; } while (0);
  if (__builtin_dynamic_object_size (q + 0, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 441); nfails++; } while (0);
  if (__builtin_dynamic_object_size (q + 64, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 443); nfails++; } while (0);
}

void
__attribute__ ((noinline))
test7 (void)
{
  struct T { char buf[10]; char buf2[10]; } t;
  char *p = &t.buf2[-4];
  char *q = &t.buf2[0];
  if (__builtin_dynamic_object_size (p, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 454); nfails++; } while (0);
  if (__builtin_dynamic_object_size (q, 3) != sizeof (t.buf2))
    do { __builtin_printf ("Failure at line: %d\n", 456); nfails++; } while (0);
  q = &t.buf[10];
  if (__builtin_dynamic_object_size (q, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 459); nfails++; } while (0);
  q = &t.buf[11];
  if (__builtin_dynamic_object_size (q, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 462); nfails++; } while (0);
  p = &t.buf[-4];
  if (__builtin_dynamic_object_size (p, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 465); nfails++; } while (0);
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


  if (__builtin_dynamic_object_size (&p[-4], 3) != (cond ? 6 : 10))
    do { __builtin_printf ("Failure at line: %d\n", 482); nfails++; } while (0);





  for (unsigned i = cond; i > 0; i--)
    p--;


  if (__builtin_dynamic_object_size (p, 3) != ((cond ? 2 : 6) + cond))
    do { __builtin_printf ("Failure at line: %d\n", 493); nfails++; } while (0);





  p = &y.c[8];
  for (unsigned i = cond; i > 0; i--)
    p--;


  if (__builtin_dynamic_object_size (p, 3) != sizeof (y.c) - 8 + cond)
    do { __builtin_printf ("Failure at line: %d\n", 505); nfails++; } while (0);




}



size_t
__attribute__ ((noinline))
test9 (void)
{
  const char *ptr = "abcdefghijklmnopqrstuvwxyz";
  char *res = strndup (ptr, 21);
  if (__builtin_dynamic_object_size (res, 3) != 22)
    do { __builtin_printf ("Failure at line: %d\n", 521); nfails++; } while (0);

  free (res);

  res = strndup (ptr, 32);
  if (__builtin_dynamic_object_size (res, 3) != 27)
    do { __builtin_printf ("Failure at line: %d\n", 527); nfails++; } while (0);

  free (res);

  res = strdup (ptr);
  if (__builtin_dynamic_object_size (res, 3) != 27)
    do { __builtin_printf ("Failure at line: %d\n", 533); nfails++; } while (0);

  free (res);

  char *ptr2 = malloc (64);
  strcpy (ptr2, ptr);

  res = strndup (ptr2, 21);
  if (__builtin_dynamic_object_size (res, 3) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 542); nfails++; } while (0);

  free (res);

  res = strndup (ptr2, 32);
  if (__builtin_dynamic_object_size (res, 3) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 548); nfails++; } while (0);

  free (res);

  res = strndup (ptr2, 128);
  if (__builtin_dynamic_object_size (res, 3) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 554); nfails++; } while (0);

  free (res);

  res = strdup (ptr2);

  if (__builtin_dynamic_object_size (res, 3) != 27)



    do { __builtin_printf ("Failure at line: %d\n", 564); nfails++; } while (0);

  free (res);
  free (ptr2);

  ptr = "abcd\0efghijklmnopqrstuvwxyz";
  res = strdup (ptr);
  if (__builtin_dynamic_object_size (res, 3) != 5)
    do { __builtin_printf ("Failure at line: %d\n", 572); nfails++; } while (0);
  free (res);

  res = strndup (ptr, 24);
  if (__builtin_dynamic_object_size (res, 3) != 5)
    do { __builtin_printf ("Failure at line: %d\n", 577); nfails++; } while (0);
  free (res);

  res = strndup (ptr, 2);
  if (__builtin_dynamic_object_size (res, 3) != 3)
    do { __builtin_printf ("Failure at line: %d\n", 582); nfails++; } while (0);
  free (res);

  res = strdup (&ptr[4]);
  if (__builtin_dynamic_object_size (res, 3) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 587); nfails++; } while (0);
  free (res);

  res = strndup (&ptr[4], 4);
  if (__builtin_dynamic_object_size (res, 3) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 592); nfails++; } while (0);
  free (res);

  res = strndup (&ptr[4], 1);
  if (__builtin_dynamic_object_size (res, 3) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 597); nfails++; } while (0);
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
# 7 "./builtin-dynamic-object-size-4.c" 2
