//type: rp
//options: 
# 0 "./ext/builtin-object-size2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/builtin-object-size2.C"



# 1 "./ext/../../gcc.dg/builtin-object-size-common.h" 1
typedef long unsigned int size_t;

extern "C" {

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

}


unsigned nfails = 0;
# 5 "./ext/builtin-object-size2.C" 2

typedef struct A
{
  char a[10];
  int b;
  char c[10];
  static int d;
} AT;

int A::d = 6;

void
__attribute__ ((noinline))
test1 (A *p)
{
  char *c;
  if (__builtin_object_size (&p->a, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 22); nfails++; } while (0);
  if (__builtin_object_size (&p->a[0], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 24); nfails++; } while (0);
  if (__builtin_object_size (&p->a[3], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 26); nfails++; } while (0);
  if (__builtin_object_size (&p->b, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 28); nfails++; } while (0);
  if (__builtin_object_size (&p->c, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 30); nfails++; } while (0);
  c = p->a;
  if (__builtin_object_size (c, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 33); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_object_size (c, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 36); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_object_size (c, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 39); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_object_size (c, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 42); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_object_size (c, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 45); nfails++; } while (0);
  if (__builtin_object_size (&p->a, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 47); nfails++; } while (0);
  if (__builtin_object_size (&p->a[0], 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 49); nfails++; } while (0);
  if (__builtin_object_size (&p->a[3], 1) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 51); nfails++; } while (0);
  if (__builtin_object_size (&p->b, 1) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 53); nfails++; } while (0);
  if (__builtin_object_size (&p->c, 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 55); nfails++; } while (0);
  c = p->a;
  if (__builtin_object_size (c, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 58); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_object_size (c, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 61); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_object_size (c, 1) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 64); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_object_size (c, 1) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 67); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_object_size (c, 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 70); nfails++; } while (0);
  if (__builtin_object_size (&p->a, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 72); nfails++; } while (0);
  if (__builtin_object_size (&p->a[0], 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 74); nfails++; } while (0);
  if (__builtin_object_size (&p->a[3], 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 76); nfails++; } while (0);
  if (__builtin_object_size (&p->b, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 78); nfails++; } while (0);
  if (__builtin_object_size (&p->c, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 80); nfails++; } while (0);
  c = p->a;
  if (__builtin_object_size (c, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 83); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_object_size (c, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_object_size (c, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 89); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_object_size (c, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 92); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_object_size (c, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 95); nfails++; } while (0);
  if (__builtin_object_size (&p->a, 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 97); nfails++; } while (0);
  if (__builtin_object_size (&p->a[0], 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 99); nfails++; } while (0);
  if (__builtin_object_size (&p->a[3], 3) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 101); nfails++; } while (0);
  if (__builtin_object_size (&p->b, 3) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 103); nfails++; } while (0);
  if (__builtin_object_size (&p->c, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 105); nfails++; } while (0);
  c = p->a;
  if (__builtin_object_size (c, 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 108); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_object_size (c, 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 111); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_object_size (c, 3) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_object_size (c, 3) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 117); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_object_size (c, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 120); nfails++; } while (0);
}

void
__attribute__ ((noinline))
test2 (void)
{
  char *c;
  size_t s = 2 * sizeof (A);
  A *p = (A *) malloc (2 * sizeof (A));
  if (__builtin_object_size (&p->a, 0) != s)
    do { __builtin_printf ("Failure at line: %d\n", 131); nfails++; } while (0);
  if (__builtin_object_size (&p->a[0], 0) != s)
    do { __builtin_printf ("Failure at line: %d\n", 133); nfails++; } while (0);
  if (__builtin_object_size (&p->a[3], 0) != s - 3)
    do { __builtin_printf ("Failure at line: %d\n", 135); nfails++; } while (0);
  if (__builtin_object_size (&p->b, 0) != s - __builtin_offsetof (A, b))
    do { __builtin_printf ("Failure at line: %d\n", 137); nfails++; } while (0);
  if (__builtin_object_size (&p->c, 0) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 139); nfails++; } while (0);
  c = p->a;
  if (__builtin_object_size (c, 0) != s)
    do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_object_size (c, 0) != s)
    do { __builtin_printf ("Failure at line: %d\n", 145); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_object_size (c, 0) != s - 3)
    do { __builtin_printf ("Failure at line: %d\n", 148); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_object_size (c, 0) != s - __builtin_offsetof (A, b))
    do { __builtin_printf ("Failure at line: %d\n", 151); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_object_size (c, 0) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 154); nfails++; } while (0);
  if (__builtin_object_size (&p->a, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 156); nfails++; } while (0);
  if (__builtin_object_size (&p->a[0], 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 158); nfails++; } while (0);
  if (__builtin_object_size (&p->a[3], 1) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 160); nfails++; } while (0);
  if (__builtin_object_size (&p->b, 1) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 162); nfails++; } while (0);
  if (__builtin_object_size (&p->c, 1) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 164); nfails++; } while (0);
  c = p->a;
  if (__builtin_object_size (c, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 167); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_object_size (c, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 170); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_object_size (c, 1) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 173); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_object_size (c, 1) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 176); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_object_size (c, 1) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0);
  if (__builtin_object_size (&p->a, 2) != s)
    do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0);
  if (__builtin_object_size (&p->a[0], 2) != s)
    do { __builtin_printf ("Failure at line: %d\n", 183); nfails++; } while (0);
  if (__builtin_object_size (&p->a[3], 2) != s - 3)
    do { __builtin_printf ("Failure at line: %d\n", 185); nfails++; } while (0);
  if (__builtin_object_size (&p->b, 2) != s - __builtin_offsetof (A, b))
    do { __builtin_printf ("Failure at line: %d\n", 187); nfails++; } while (0);
  if (__builtin_object_size (&p->c, 2) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 189); nfails++; } while (0);
  c = p->a;
  if (__builtin_object_size (c, 2) != s)
    do { __builtin_printf ("Failure at line: %d\n", 192); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_object_size (c, 2) != s)
    do { __builtin_printf ("Failure at line: %d\n", 195); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_object_size (c, 2) != s - 3)
    do { __builtin_printf ("Failure at line: %d\n", 198); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_object_size (c, 2) != s - __builtin_offsetof (A, b))
    do { __builtin_printf ("Failure at line: %d\n", 201); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_object_size (c, 2) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 204); nfails++; } while (0);
  if (__builtin_object_size (&p->a, 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 206); nfails++; } while (0);
  if (__builtin_object_size (&p->a[0], 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 208); nfails++; } while (0);
  if (__builtin_object_size (&p->a[3], 3) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 210); nfails++; } while (0);
  if (__builtin_object_size (&p->b, 3) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 212); nfails++; } while (0);
  if (__builtin_object_size (&p->c, 3) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 214); nfails++; } while (0);
  c = p->a;
  if (__builtin_object_size (c, 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 217); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_object_size (c, 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 220); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_object_size (c, 3) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 223); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_object_size (c, 3) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 226); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_object_size (c, 3) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 229); nfails++; } while (0);
  free (p);
}

void
__attribute__ ((noinline))
test3 (void)
{
  char *c;
  size_t s;
  A *p = (A *) malloc (4);
  if (__builtin_object_size (&p->a, 0) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 241); nfails++; } while (0);
  if (__builtin_object_size (&p->a[0], 0) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 243); nfails++; } while (0);
  if (__builtin_object_size (&p->a[3], 0) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 245); nfails++; } while (0);
  if (__builtin_object_size (&p->b, 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 247); nfails++; } while (0);
  if (__builtin_object_size (&p->c, 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 249); nfails++; } while (0);
  if (__builtin_object_size (&p->a, 1) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 251); nfails++; } while (0);
  if (__builtin_object_size (&p->a[0], 1) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 253); nfails++; } while (0);
  if (__builtin_object_size (&p->a[3], 1) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 255); nfails++; } while (0);
  if (__builtin_object_size (&p->b, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 257); nfails++; } while (0);
  if (__builtin_object_size (&p->c, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 259); nfails++; } while (0);
  free (p);
  s = __builtin_offsetof (A, c) + 4;
  p = (A *) malloc (s);
  if (__builtin_object_size (&p->a, 0) != s)
    do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0);
  if (__builtin_object_size (&p->a[0], 0) != s)
    do { __builtin_printf ("Failure at line: %d\n", 266); nfails++; } while (0);
  if (__builtin_object_size (&p->a[3], 0) != s - 3)
    do { __builtin_printf ("Failure at line: %d\n", 268); nfails++; } while (0);
  if (__builtin_object_size (&p->b, 0) != s - __builtin_offsetof (A, b))
    do { __builtin_printf ("Failure at line: %d\n", 270); nfails++; } while (0);
  if (__builtin_object_size (&p->c, 0) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 272); nfails++; } while (0);
  if (__builtin_object_size (&p->a, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 274); nfails++; } while (0);
  if (__builtin_object_size (&p->a[0], 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 276); nfails++; } while (0);
  if (__builtin_object_size (&p->a[3], 1) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 278); nfails++; } while (0);
  if (__builtin_object_size (&p->b, 1) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 280); nfails++; } while (0);
  if (__builtin_object_size (&p->c, 1) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 282); nfails++; } while (0);
  free (p);
}

struct B
{
  A a[4];
};

void
__attribute__ ((noinline))
test4 (struct B *q, int i)
{
  if (__builtin_object_size (&q->a[2].a[2], 1) != sizeof (q->a[0].a) - 2)
    do { __builtin_printf ("Failure at line: %d\n", 296); nfails++; } while (0);
  if (__builtin_object_size (&q->a[2].c[2], 1) != sizeof (q->a[0].c) - 2)
    do { __builtin_printf ("Failure at line: %d\n", 298); nfails++; } while (0);
  if (__builtin_object_size (&q->a[3].a[2], 1) != sizeof (q->a[0].a) - 2)
    do { __builtin_printf ("Failure at line: %d\n", 300); nfails++; } while (0);
  if (__builtin_object_size (&q->a[3].c[2], 1) != sizeof (q->a[0].c) - 2)
    do { __builtin_printf ("Failure at line: %d\n", 302); nfails++; } while (0);
  if (__builtin_object_size (&q->a[i].a[2], 1) != sizeof (q->a[0].a) - 2)
    do { __builtin_printf ("Failure at line: %d\n", 304); nfails++; } while (0);
  if (__builtin_object_size (&q->a[i].c[2], 1) != sizeof (q->a[0].c) - 2)
    do { __builtin_printf ("Failure at line: %d\n", 306); nfails++; } while (0);
}

struct C
{
  char a[10];
  char b;
};

void
__attribute__ ((noinline))
test5 (struct C *c)
{
  if (__builtin_object_size (&c->b, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 320); nfails++; } while (0);
  if (__builtin_object_size (&c->b, 1) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 322); nfails++; } while (0);
  if (__builtin_object_size (&c->b, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 324); nfails++; } while (0);
  if (__builtin_object_size (&c->b, 3) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 326); nfails++; } while (0);
}

struct D
{
  int i;
  struct D1
  {
    char b;
    char a[10];
  } j;
};

void
__attribute__ ((noinline))
test6 (struct D *d)
{
  if (__builtin_object_size (&d->j.a[3], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 344); nfails++; } while (0);
  if (__builtin_object_size (&d->j.a[3], 1) != sizeof (d->j.a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 346); nfails++; } while (0);
  if (__builtin_object_size (&d->j.a[3], 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 348); nfails++; } while (0);
  if (__builtin_object_size (&d->j.a[3], 3) != sizeof (d->j.a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 350); nfails++; } while (0);
}

struct E
{
  int i;
  struct E1
  {
    char b;
    char a[10];
  } j[1];
};

void
__attribute__ ((noinline))
test7 (struct E *e)
{
  if (__builtin_object_size (&e->j[0].a[3], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 368); nfails++; } while (0);
  if (__builtin_object_size (&e->j[0].a[3], 1) != sizeof (e->j[0].a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 370); nfails++; } while (0);
  if (__builtin_object_size (&e->j[0].a[3], 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 372); nfails++; } while (0);
  if (__builtin_object_size (&e->j[0].a[3], 3) != sizeof (e->j[0].a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 374); nfails++; } while (0);
  if (__builtin_object_size ((char *) &e->j[0], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 376); nfails++; } while (0);
  if (__builtin_object_size ((char *) &e->j[0], 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 378); nfails++; } while (0);
  if (__builtin_object_size ((char *) &e->j[0], 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 380); nfails++; } while (0);
  if (__builtin_object_size ((char *) &e->j[0], 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 382); nfails++; } while (0);
}

union F
{
  char a[1];
  struct F1
  {
    char b;
    char c[10];
  } d;
};

void
__attribute__ ((noinline))
test8 (union F *f)
{
  if (__builtin_object_size (&f->d.c[3], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 400); nfails++; } while (0);
  if (__builtin_object_size (&f->d.c[3], 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 402); nfails++; } while (0);
  if (__builtin_object_size (&f->d.c[3], 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 404); nfails++; } while (0);
  if (__builtin_object_size (&f->d.c[3], 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 406); nfails++; } while (0);
}




void
__attribute__ ((noinline))
test9 (void)
{
  char line[256];
  const char *p = "bbbbbbbbbbbbbbbbbbbbbbbbbbb";
  const char *q = p + sizeof ("bbbbbbbbbbbbbbbbbbbbbbbbbbb") - 1;

  char *q1 = line;
  for (const char *p1 = p; p1 < q;)
    {
      *q1++ = *p1++;

      if (p1 < q && (*q1++ = *p1++) != '\0')
 {
   if (__builtin_object_size (q1 - 2, 0) == 0)
     __builtin_abort ();
   if (__builtin_object_size (q1 - 2, 1) == 0)
     __builtin_abort ();
 }
    }
}

int
main (void)
{
  A a, *p = &a;
  int i = 1;
  __asm ("" : "+r" (p));
  test1 (p);
  test2 ();
  test3 ();
  struct B b, *q = &b;
  __asm ("" : "+r" (q), "+r" (i));
  test4 (q, i);
  struct C c, *cp = &c;
  __asm ("" : "+r" (cp));
  test5 (cp);
  struct D d, *dp = &d;
  __asm ("" : "+r" (dp));
  test6 (dp);
  struct E e, *ep = &e;
  __asm ("" : "+r" (ep));
  test7 (ep);
  union F f, *fp = &f;
  __asm ("" : "+r" (fp));
  test8 (fp);
  test9 ();
  do { if (nfails > 0) __builtin_abort (); return 0; } while (0);
}
