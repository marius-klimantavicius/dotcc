//type: rp
//options: 
# 0 "./builtin-object-size-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-object-size-1.c"




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
# 6 "./builtin-object-size-1.c" 2

struct A
{
  char a[10];
  int b;
  char c[10];
} y, w[4];

extern char exta[];
extern char extb[30];
extern struct A zerol[0];

void
__attribute__ ((noinline))
test1 (void *q, int x)
{
  struct A a;
  void *p = &a.a[3], *r;
  char var[x + 10];
  if (x < 0)
    r = &a.a[9];
  else
    r = &a.c[1];
  if (__builtin_object_size (p, 0)
      != sizeof (a) - __builtin_offsetof (struct A, a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 31); nfails++; } while (0);
  if (__builtin_object_size (&a.c[9], 0)
      != sizeof (a) - __builtin_offsetof (struct A, c) - 9)
    do { __builtin_printf ("Failure at line: %d\n", 34); nfails++; } while (0);
  if (__builtin_object_size (q, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 36); nfails++; } while (0);







  if (__builtin_object_size (r, 0)
      != sizeof (a) - __builtin_offsetof (struct A, a) - 9)
    do { __builtin_printf ("Failure at line: %d\n", 46); nfails++; } while (0);

  if (x < 6)
    r = &w[2].a[1];
  else
    r = &a.a[6];
  if (__builtin_object_size (&y, 0)
      != sizeof (y))
    do { __builtin_printf ("Failure at line: %d\n", 54); nfails++; } while (0);
  if (__builtin_object_size (w, 0)
      != sizeof (w))
    do { __builtin_printf ("Failure at line: %d\n", 57); nfails++; } while (0);
  if (__builtin_object_size (&y.b, 0)
      != sizeof (a) - __builtin_offsetof (struct A, b))
    do { __builtin_printf ("Failure at line: %d\n", 60); nfails++; } while (0);







  if (__builtin_object_size (r, 0)
      != 2 * sizeof (w[0]) - __builtin_offsetof (struct A, a) - 1)
    do { __builtin_printf ("Failure at line: %d\n", 70); nfails++; } while (0);

  if (x < 20)
    r = malloc (30);
  else
    r = calloc (2, 16);







  if (__builtin_object_size (r, 0) != 2 * 16
      && __builtin_object_size (r, 0) != 30)
    do { __builtin_printf ("Failure at line: %d\n", 85); nfails++; } while (0);

  if (x < 20)
    r = malloc (30);
  else
    r = calloc (2, 14);




  if (__builtin_object_size (r, 0) != 30)
    do { __builtin_printf ("Failure at line: %d\n", 96); nfails++; } while (0);

  if (x < 30)
    r = malloc (sizeof (a));
  else
    r = &a.a[3];




  if (__builtin_object_size (r, 0) != sizeof (a))
    do { __builtin_printf ("Failure at line: %d\n", 107); nfails++; } while (0);

  r = memcpy (r, "a", 2);




  if (__builtin_object_size (r, 0) != sizeof (a))
    do { __builtin_printf ("Failure at line: %d\n", 115); nfails++; } while (0);

  r = memcpy (r + 2, "b", 2) + 2;





  if (__builtin_object_size (r, 0) != sizeof (a) - 4)
    do { __builtin_printf ("Failure at line: %d\n", 124); nfails++; } while (0);

  r = &a.a[4];
  r = memset (r, 'a', 2);
  if (__builtin_object_size (r, 0)
      != sizeof (a) - __builtin_offsetof (struct A, a) - 4)
    do { __builtin_printf ("Failure at line: %d\n", 130); nfails++; } while (0);
  r = memset (r + 2, 'b', 2) + 2;
  if (__builtin_object_size (r, 0)
      != sizeof (a) - __builtin_offsetof (struct A, a) - 8)
    do { __builtin_printf ("Failure at line: %d\n", 134); nfails++; } while (0);
  r = &a.a[1];
  r = strcpy (r, "ab");
  if (__builtin_object_size (r, 0)
      != sizeof (a) - __builtin_offsetof (struct A, a) - 1)
    do { __builtin_printf ("Failure at line: %d\n", 139); nfails++; } while (0);
  r = strcpy (r + 2, "cd") + 2;
  if (__builtin_object_size (r, 0)
      != sizeof (a) - __builtin_offsetof (struct A, a) - 5)
    do { __builtin_printf ("Failure at line: %d\n", 143); nfails++; } while (0);
  if (__builtin_object_size (exta, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 145); nfails++; } while (0);
  if (__builtin_object_size (exta + 10, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 147); nfails++; } while (0);
  if (__builtin_object_size (&exta[5], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 149); nfails++; } while (0);
  if (__builtin_object_size (extb, 0) != sizeof (extb))
    do { __builtin_printf ("Failure at line: %d\n", 151); nfails++; } while (0);
  if (__builtin_object_size (extb + 10, 0) != sizeof (extb) - 10)
    do { __builtin_printf ("Failure at line: %d\n", 153); nfails++; } while (0);
  if (__builtin_object_size (&extb[5], 0) != sizeof (extb) - 5)
    do { __builtin_printf ("Failure at line: %d\n", 155); nfails++; } while (0);
# 164 "./builtin-object-size-1.c"
  if (__builtin_object_size (var, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 165); nfails++; } while (0);
  if (__builtin_object_size (var + 10, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 167); nfails++; } while (0);
  if (__builtin_object_size (&var[5], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 169); nfails++; } while (0);

  if (__builtin_object_size (zerol, 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 172); nfails++; } while (0);
  if (__builtin_object_size (&zerol, 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 174); nfails++; } while (0);
  if (__builtin_object_size (&zerol[0], 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 176); nfails++; } while (0);
  if (__builtin_object_size (zerol[0].a, 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 178); nfails++; } while (0);
  if (__builtin_object_size (&zerol[0].a[0], 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 180); nfails++; } while (0);
  if (__builtin_object_size (&zerol[0].b, 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 182); nfails++; } while (0);
  if (__builtin_object_size ("abcdefg", 0) != sizeof ("abcdefg"))
    do { __builtin_printf ("Failure at line: %d\n", 184); nfails++; } while (0);
  if (__builtin_object_size ("abcd\0efg", 0) != sizeof ("abcd\0efg"))
    do { __builtin_printf ("Failure at line: %d\n", 186); nfails++; } while (0);
  if (__builtin_object_size (&"abcd\0efg", 0) != sizeof ("abcd\0efg"))
    do { __builtin_printf ("Failure at line: %d\n", 188); nfails++; } while (0);
  if (__builtin_object_size (&"abcd\0efg"[0], 0) != sizeof ("abcd\0efg"))
    do { __builtin_printf ("Failure at line: %d\n", 190); nfails++; } while (0);
  if (__builtin_object_size (&"abcd\0efg"[4], 0) != sizeof ("abcd\0efg") - 4)
    do { __builtin_printf ("Failure at line: %d\n", 192); nfails++; } while (0);
  if (__builtin_object_size ("abcd\0efg" + 5, 0) != sizeof ("abcd\0efg") - 5)
    do { __builtin_printf ("Failure at line: %d\n", 194); nfails++; } while (0);
  if (__builtin_object_size (L"abcdefg", 0) != sizeof (L"abcdefg"))
    do { __builtin_printf ("Failure at line: %d\n", 196); nfails++; } while (0);
  r = (char *) L"abcd\0efg";
  if (__builtin_object_size (r + 2, 0) != sizeof (L"abcd\0efg") - 2)
    do { __builtin_printf ("Failure at line: %d\n", 199); nfails++; } while (0);
}

size_t l1 = 1;

void
__attribute__ ((noinline))
test2 (void)
{
  struct B { char buf1[10]; char buf2[10]; } a;
  char *r, buf3[20];
  int i;
  size_t res;

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
# 243 "./builtin-object-size-1.c"
  res = 20;

  if (__builtin_object_size (r, 0) != res)
    do { __builtin_printf ("Failure at line: %d\n", 246); nfails++; } while (0);
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
# 276 "./builtin-object-size-1.c"
  res = 15;

  if (__builtin_object_size (r, 0) != res)
    do { __builtin_printf ("Failure at line: %d\n", 279); nfails++; } while (0);
  r += 8;
# 293 "./builtin-object-size-1.c"
  if (__builtin_object_size (r, 0) != 7)
    do { __builtin_printf ("Failure at line: %d\n", 294); nfails++; } while (0);
  if (__builtin_object_size (r + 6, 0) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 296); nfails++; } while (0);

  r = &buf3[18];
  for (i = 0; i < 4; ++i)
    {
      if (i == l1 - 1)
 r = &a.buf1[9];
      else if (i == l1)
 r = &a.buf2[9];
      else if (i == l1 + 1)
 r = &buf3[5];
      else if (i == l1 + 2)
 r = &a.buf1[4];
    }
# 332 "./builtin-object-size-1.c"
  if (__builtin_object_size (r + 12, 0) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0);

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

  if (__builtin_object_size (buf4, 0) != sizeof (buf4))
    do { __builtin_printf ("Failure at line: %d\n", 349); nfails++; } while (0);
  if (__builtin_object_size (&buf4, 0) != sizeof (buf4))
    do { __builtin_printf ("Failure at line: %d\n", 351); nfails++; } while (0);
  if (__builtin_object_size (&buf4[0], 0) != sizeof (buf4))
    do { __builtin_printf ("Failure at line: %d\n", 353); nfails++; } while (0);
  if (__builtin_object_size (&buf4[1], 0) != sizeof (buf4) - 1)
    do { __builtin_printf ("Failure at line: %d\n", 355); nfails++; } while (0);
  if (__builtin_object_size (&x, 0) != sizeof (x))
    do { __builtin_printf ("Failure at line: %d\n", 357); nfails++; } while (0);
  if (__builtin_object_size (&x.a, 0) != sizeof (x))
    do { __builtin_printf ("Failure at line: %d\n", 359); nfails++; } while (0);
  if (__builtin_object_size (&x.a[0], 0) != sizeof (x))
    do { __builtin_printf ("Failure at line: %d\n", 361); nfails++; } while (0);
  if (__builtin_object_size (&x.a[0].a, 0) != sizeof (x))
    do { __builtin_printf ("Failure at line: %d\n", 363); nfails++; } while (0);
  if (__builtin_object_size (&x.a[0].a[0], 0) != sizeof (x))
    do { __builtin_printf ("Failure at line: %d\n", 365); nfails++; } while (0);
  if (__builtin_object_size (&x.a[0].a[3], 0) != sizeof (x) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 367); nfails++; } while (0);
  if (__builtin_object_size (&x.a[0].b, 0)
      != sizeof (x) - __builtin_offsetof (struct A, b))
    do { __builtin_printf ("Failure at line: %d\n", 370); nfails++; } while (0);
  if (__builtin_object_size (&x.a[1].c, 0)
      != sizeof (x) - sizeof (struct A) - __builtin_offsetof (struct A, c))
    do { __builtin_printf ("Failure at line: %d\n", 373); nfails++; } while (0);
  if (__builtin_object_size (&x.a[1].c[0], 0)
      != sizeof (x) - sizeof (struct A) - __builtin_offsetof (struct A, c))
    do { __builtin_printf ("Failure at line: %d\n", 376); nfails++; } while (0);
  if (__builtin_object_size (&x.a[1].c[3], 0)
      != sizeof (x) - sizeof (struct A) - __builtin_offsetof (struct A, c) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 379); nfails++; } while (0);
  if (__builtin_object_size (&x.b, 0)
      != sizeof (x) - __builtin_offsetof (struct B, b))
    do { __builtin_printf ("Failure at line: %d\n", 382); nfails++; } while (0);
  if (__builtin_object_size (&x.b.a, 0)
      != sizeof (x) - __builtin_offsetof (struct B, b))
    do { __builtin_printf ("Failure at line: %d\n", 385); nfails++; } while (0);
  if (__builtin_object_size (&x.b.a[0], 0)
      != sizeof (x) - __builtin_offsetof (struct B, b))
    do { __builtin_printf ("Failure at line: %d\n", 388); nfails++; } while (0);
  if (__builtin_object_size (&x.b.a[3], 0)
      != sizeof (x) - __builtin_offsetof (struct B, b) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 391); nfails++; } while (0);
  if (__builtin_object_size (&x.b.b, 0)
      != sizeof (x) - __builtin_offsetof (struct B, b)
  - __builtin_offsetof (struct A, b))
    do { __builtin_printf ("Failure at line: %d\n", 395); nfails++; } while (0);
  if (__builtin_object_size (&x.b.c, 0)
      != sizeof (x) - __builtin_offsetof (struct B, b)
  - __builtin_offsetof (struct A, c))
    do { __builtin_printf ("Failure at line: %d\n", 399); nfails++; } while (0);
  if (__builtin_object_size (&x.b.c[0], 0)
      != sizeof (x) - __builtin_offsetof (struct B, b)
  - __builtin_offsetof (struct A, c))
    do { __builtin_printf ("Failure at line: %d\n", 403); nfails++; } while (0);
  if (__builtin_object_size (&x.b.c[3], 0)
      != sizeof (x) - __builtin_offsetof (struct B, b)
  - __builtin_offsetof (struct A, c) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 407); nfails++; } while (0);
  if (__builtin_object_size (&x.c, 0)
      != sizeof (x) - __builtin_offsetof (struct B, c))
    do { __builtin_printf ("Failure at line: %d\n", 410); nfails++; } while (0);
  if (__builtin_object_size (&x.c[0], 0)
      != sizeof (x) - __builtin_offsetof (struct B, c))
    do { __builtin_printf ("Failure at line: %d\n", 413); nfails++; } while (0);
  if (__builtin_object_size (&x.c[1], 0)
      != sizeof (x) - __builtin_offsetof (struct B, c) - 1)
    do { __builtin_printf ("Failure at line: %d\n", 416); nfails++; } while (0);
  if (__builtin_object_size (&x.d, 0)
      != sizeof (x) - __builtin_offsetof (struct B, d))
    do { __builtin_printf ("Failure at line: %d\n", 419); nfails++; } while (0);
  if (__builtin_object_size (&x.e, 0)
      != sizeof (x) - __builtin_offsetof (struct B, e))
    do { __builtin_printf ("Failure at line: %d\n", 422); nfails++; } while (0);
  if (__builtin_object_size (&x.f, 0)
      != sizeof (x) - __builtin_offsetof (struct B, f))
    do { __builtin_printf ("Failure at line: %d\n", 425); nfails++; } while (0);
  dp = &__real__ x.f;
  if (__builtin_object_size (dp, 0)
      != sizeof (x) - __builtin_offsetof (struct B, f))
    do { __builtin_printf ("Failure at line: %d\n", 429); nfails++; } while (0);
  dp = &__imag__ x.f;
  if (__builtin_object_size (dp, 0)
      != sizeof (x) - __builtin_offsetof (struct B, f)
  - sizeof (x.f) / 2)
    do { __builtin_printf ("Failure at line: %d\n", 434); nfails++; } while (0);
  dp = &y;
  if (__builtin_object_size (dp, 0) != sizeof (y))
    do { __builtin_printf ("Failure at line: %d\n", 437); nfails++; } while (0);
  if (__builtin_object_size (&z, 0) != sizeof (z))
    do { __builtin_printf ("Failure at line: %d\n", 439); nfails++; } while (0);
  dp = &__real__ z;
  if (__builtin_object_size (dp, 0) != sizeof (z))
    do { __builtin_printf ("Failure at line: %d\n", 442); nfails++; } while (0);
  dp = &__imag__ z;
  if (__builtin_object_size (dp, 0) != sizeof (z) / 2)
    do { __builtin_printf ("Failure at line: %d\n", 445); nfails++; } while (0);
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
      if (__builtin_object_size (p, 0) != (size_t) -1)
 do { __builtin_printf ("Failure at line: %d\n", 462); nfails++; } while (0);
    }
  return x;
}

void
__attribute__ ((noinline))
test5 (size_t x)
{
  char buf[64];
  char *p = &buf[8];
  size_t i;

  for (i = 0; i < x; ++i)
    p = p + 4;
# 488 "./builtin-object-size-1.c"
  if (__builtin_object_size (p, 0) != sizeof (buf) - 8)
    do { __builtin_printf ("Failure at line: %d\n", 489); nfails++; } while (0);

  memset (p, ' ', sizeof (buf) - 8 - 4 * 4);
}

void
__attribute__ ((noinline))
test6 (size_t x)
{
  struct T { char buf[64]; char buf2[64]; } t;
  char *p = &t.buf[8];
  size_t i;

  for (i = 0; i < x; ++i)
    p = p + 4;




  if (__builtin_object_size (p, 0) != sizeof (t) - 8)
    do { __builtin_printf ("Failure at line: %d\n", 509); nfails++; } while (0);

  memset (p, ' ', sizeof (t) - 8 - 4 * 4);
}

void
__attribute__ ((noinline))
test7 (void)
{
  char buf[64];
  struct T { char buf[64]; char buf2[64]; } t;
  char *p = &buf[64], *q = &t.buf[64];

  if (__builtin_object_size (p + 64, 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 523); nfails++; } while (0);
  if (__builtin_object_size (q + 63, 0) != sizeof (t) - 64 - 63)
    do { __builtin_printf ("Failure at line: %d\n", 525); nfails++; } while (0);
  if (__builtin_object_size (q + 64, 0) != sizeof (t) - 64 - 64)
    do { __builtin_printf ("Failure at line: %d\n", 527); nfails++; } while (0);
  if (__builtin_object_size (q + 256, 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 529); nfails++; } while (0);
}

void
__attribute__ ((noinline))
test8 (void)
{
  struct T { char buf[10]; char buf2[10]; } t;
  char *p = &t.buf2[-4];
  char *q = &t.buf2[0];
  if (__builtin_object_size (p, 0) != sizeof (t) - 10 + 4)
    do { __builtin_printf ("Failure at line: %d\n", 540); nfails++; } while (0);
  if (__builtin_object_size (q, 0) != sizeof (t) - 10)
    do { __builtin_printf ("Failure at line: %d\n", 542); nfails++; } while (0);

  q = q - 8;
  if (__builtin_object_size (q, 0) != (size_t) -1
      && __builtin_object_size (q, 0) != sizeof (t) - 10 + 8)
    do { __builtin_printf ("Failure at line: %d\n", 547); nfails++; } while (0);
  p = &t.buf[-4];
  if (__builtin_object_size (p, 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 550); nfails++; } while (0);
}

void
__attribute__ ((noinline))
test9 (unsigned cond)
{
  char *buf2 = malloc (10);
  char *p;

  if (cond)
    p = &buf2[8];
  else
    p = &buf2[4];





  if (__builtin_object_size (&p[-4], 0) != 10)
    do { __builtin_printf ("Failure at line: %d\n", 570); nfails++; } while (0);


  for (unsigned i = cond; i > 0; i--)
    p--;





  if (__builtin_object_size (p, 0) != 10)
    do { __builtin_printf ("Failure at line: %d\n", 581); nfails++; } while (0);


  p = &y.c[8];
  for (unsigned i = cond; i > 0; i--)
    p--;






  if (__builtin_object_size (p, 0) != sizeof (y))
    do { __builtin_printf ("Failure at line: %d\n", 594); nfails++; } while (0);

}

void
__attribute__ ((noinline))
test10 (void)
{
  static char buf[255];
  unsigned int i, len = sizeof (buf);
  char *p = buf;

  for (i = 0 ; i < sizeof (buf) ; i++)
    {
      if (len < 2)
 {




   if (__builtin_object_size (p - 3, 0) != sizeof (buf))
     do { __builtin_printf ("Failure at line: %d\n", 615); nfails++; } while (0);

   break;
 }
      p++;
      len--;
    }
}



size_t
__attribute__ ((noinline))
test11 (void)
{
  int i = 0;
  const char *ptr = "abcdefghijklmnopqrstuvwxyz";
  char *res = strndup (ptr, 21);
  if (__builtin_object_size (res, 0) != 22)
    do { __builtin_printf ("Failure at line: %d\n", 634); nfails++; } while (0);

  free (res);

  res = strndup (ptr, 32);
  if (__builtin_object_size (res, 0) != 27)
    do { __builtin_printf ("Failure at line: %d\n", 640); nfails++; } while (0);

  free (res);

  res = strdup (ptr);
  if (__builtin_object_size (res, 0) != 27)
    do { __builtin_printf ("Failure at line: %d\n", 646); nfails++; } while (0);

  free (res);

  char *ptr2 = malloc (64);
  strcpy (ptr2, ptr);

  res = strndup (ptr2, 21);
  if (__builtin_object_size (res, 0) != 22)
    do { __builtin_printf ("Failure at line: %d\n", 655); nfails++; } while (0);

  free (res);

  res = strndup (ptr2, 32);
  if (__builtin_object_size (res, 0) != 33)
    do { __builtin_printf ("Failure at line: %d\n", 661); nfails++; } while (0);

  free (res);

  res = strndup (ptr2, 128);
  if (__builtin_object_size (res, 0) != 64)
    do { __builtin_printf ("Failure at line: %d\n", 667); nfails++; } while (0);

  free (res);

  res = strdup (ptr2);



  if (__builtin_object_size (res, 0) != (size_t) -1)

    do { __builtin_printf ("Failure at line: %d\n", 677); nfails++; } while (0);
  free (res);
  free (ptr2);

  ptr = "abcd\0efghijklmnopqrstuvwxyz";
  res = strdup (ptr);
  if (__builtin_object_size (res, 0) != 5)
    do { __builtin_printf ("Failure at line: %d\n", 684); nfails++; } while (0);
  free (res);

  res = strndup (ptr, 24);
  if (__builtin_object_size (res, 0) != 5)
    do { __builtin_printf ("Failure at line: %d\n", 689); nfails++; } while (0);
  free (res);

  res = strndup (ptr, 2);
  if (__builtin_object_size (res, 0) != 3)
    do { __builtin_printf ("Failure at line: %d\n", 694); nfails++; } while (0);
  free (res);

  res = strdup (&ptr[4]);
  if (__builtin_object_size (res, 0) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 699); nfails++; } while (0);
  free (res);

  res = strndup (&ptr[4], 4);
  if (__builtin_object_size (res, 0) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 704); nfails++; } while (0);
  free (res);

  res = strndup (&ptr[4], 1);
  if (__builtin_object_size (res, 0) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 709); nfails++; } while (0);
  free (res);
}


void
__attribute__ ((noinline))
test12 (unsigned off)
{
  char *buf2 = malloc (10);
  char *p;
  size_t t;

  p = &buf2[off];





  if (__builtin_object_size (p, 0) != 10)
    do { __builtin_printf ("Failure at line: %d\n", 729); nfails++; } while (0);

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
  test6 (4);
  test7 ();
  test8 ();
  test9 (1);
  test10 ();

  test11 ();

  test12 (0);
  test12 (2);
  do { if (nfails > 0) __builtin_abort (); return 0; } while (0);
}
