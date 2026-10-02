//type: rp
//options: 
# 0 "./ext/builtin-dynamic-object-size1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/builtin-dynamic-object-size1.C"




# 1 "./ext/builtin-object-size1.C" 1



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
# 5 "./ext/builtin-object-size1.C" 2

struct A
{
  char a[10];
  int b;
  char c[10];
};

void
__attribute__ ((noinline))
test1 (A *p)
{
  char *c;
  if (__builtin_dynamic_object_size (&p->a, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 19); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[0], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 21); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[3], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 23); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->b, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 25); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->c, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 27); nfails++; } while (0);
  c = p->a;
  if (__builtin_dynamic_object_size (c, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 30); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_dynamic_object_size (c, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 33); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_dynamic_object_size (c, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 36); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_dynamic_object_size (c, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 39); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_dynamic_object_size (c, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 42); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 44); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[0], 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 46); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[3], 1) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 48); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->b, 1) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 50); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->c, 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 52); nfails++; } while (0);
  c = p->a;
  if (__builtin_dynamic_object_size (c, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 55); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_dynamic_object_size (c, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 58); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_dynamic_object_size (c, 1) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 61); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_dynamic_object_size (c, 1) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 64); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_dynamic_object_size (c, 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 67); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 69); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[0], 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 71); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[3], 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 73); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->b, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 75); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->c, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 77); nfails++; } while (0);
  c = p->a;
  if (__builtin_dynamic_object_size (c, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 80); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_dynamic_object_size (c, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 83); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_dynamic_object_size (c, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_dynamic_object_size (c, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 89); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_dynamic_object_size (c, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 92); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a, 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 94); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[0], 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 96); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[3], 3) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 98); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->b, 3) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 100); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->c, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 102); nfails++; } while (0);
  c = p->a;
  if (__builtin_dynamic_object_size (c, 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 105); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_dynamic_object_size (c, 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 108); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_dynamic_object_size (c, 3) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 111); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_dynamic_object_size (c, 3) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_dynamic_object_size (c, 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 117); nfails++; } while (0);
}

void
__attribute__ ((noinline))
test2 (void)
{
  char *c;
  size_t s = 2 * sizeof (A);
  A *p = (A *) malloc (2 * sizeof (A));
  if (__builtin_dynamic_object_size (&p->a, 0) != s)
    do { __builtin_printf ("Failure at line: %d\n", 128); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[0], 0) != s)
    do { __builtin_printf ("Failure at line: %d\n", 130); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[3], 0) != s - 3)
    do { __builtin_printf ("Failure at line: %d\n", 132); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->b, 0) != s - __builtin_offsetof (A, b))
    do { __builtin_printf ("Failure at line: %d\n", 134); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->c, 0) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 136); nfails++; } while (0);
  c = p->a;
  if (__builtin_dynamic_object_size (c, 0) != s)
    do { __builtin_printf ("Failure at line: %d\n", 139); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_dynamic_object_size (c, 0) != s)
    do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_dynamic_object_size (c, 0) != s - 3)
    do { __builtin_printf ("Failure at line: %d\n", 145); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_dynamic_object_size (c, 0) != s - __builtin_offsetof (A, b))
    do { __builtin_printf ("Failure at line: %d\n", 148); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_dynamic_object_size (c, 0) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 151); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 153); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[0], 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 155); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[3], 1) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 157); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->b, 1) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 159); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->c, 1) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 161); nfails++; } while (0);
  c = p->a;
  if (__builtin_dynamic_object_size (c, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 164); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_dynamic_object_size (c, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 167); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_dynamic_object_size (c, 1) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 170); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_dynamic_object_size (c, 1) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 173); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_dynamic_object_size (c, 1) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 176); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a, 2) != s)
    do { __builtin_printf ("Failure at line: %d\n", 178); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[0], 2) != s)
    do { __builtin_printf ("Failure at line: %d\n", 180); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[3], 2) != s - 3)
    do { __builtin_printf ("Failure at line: %d\n", 182); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->b, 2) != s - __builtin_offsetof (A, b))
    do { __builtin_printf ("Failure at line: %d\n", 184); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->c, 2) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 186); nfails++; } while (0);
  c = p->a;
  if (__builtin_dynamic_object_size (c, 2) != s)
    do { __builtin_printf ("Failure at line: %d\n", 189); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_dynamic_object_size (c, 2) != s)
    do { __builtin_printf ("Failure at line: %d\n", 192); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_dynamic_object_size (c, 2) != s - 3)
    do { __builtin_printf ("Failure at line: %d\n", 195); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_dynamic_object_size (c, 2) != s - __builtin_offsetof (A, b))
    do { __builtin_printf ("Failure at line: %d\n", 198); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_dynamic_object_size (c, 2) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 201); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a, 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 203); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[0], 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 205); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[3], 3) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->b, 3) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->c, 3) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 211); nfails++; } while (0);
  c = p->a;
  if (__builtin_dynamic_object_size (c, 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 214); nfails++; } while (0);
  c = &p->a[0];
  if (__builtin_dynamic_object_size (c, 3) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 217); nfails++; } while (0);
  c = &p->a[3];
  if (__builtin_dynamic_object_size (c, 3) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 220); nfails++; } while (0);
  c = (char *) &p->b;
  if (__builtin_dynamic_object_size (c, 3) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 223); nfails++; } while (0);
  c = (char *) &p->c;
  if (__builtin_dynamic_object_size (c, 3) != s - __builtin_offsetof (A, c))
    do { __builtin_printf ("Failure at line: %d\n", 226); nfails++; } while (0);
  free (p);
}

void
__attribute__ ((noinline))
test3 (void)
{
  char *c;
  size_t s;
  A *p = (A *) malloc (4);
  if (__builtin_dynamic_object_size (&p->a, 0) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 238); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[0], 0) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 240); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[3], 0) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 242); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->b, 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 244); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->c, 0) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 246); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a, 1) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 248); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[0], 1) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 250); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[3], 1) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 252); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->b, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 254); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->c, 1) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 256); nfails++; } while (0);
  free (p);
  s = __builtin_offsetof (A, c) + 4;
  p = (A *) malloc (s);
  if (__builtin_dynamic_object_size (&p->a, 0) != s)
    do { __builtin_printf ("Failure at line: %d\n", 261); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[0], 0) != s)
    do { __builtin_printf ("Failure at line: %d\n", 263); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[3], 0) != s - 3)
    do { __builtin_printf ("Failure at line: %d\n", 265); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->b, 0) != s - __builtin_offsetof (A, b))
    do { __builtin_printf ("Failure at line: %d\n", 267); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->c, 0) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 269); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a, 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 271); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[0], 1) != sizeof (p->a))
    do { __builtin_printf ("Failure at line: %d\n", 273); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->a[3], 1) != sizeof (p->a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 275); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->b, 1) != sizeof (p->b))
    do { __builtin_printf ("Failure at line: %d\n", 277); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&p->c, 1) != 4)
    do { __builtin_printf ("Failure at line: %d\n", 279); nfails++; } while (0);
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
  if (__builtin_dynamic_object_size (&q->a[2].a[2], 1) != sizeof (q->a[0].a) - 2)
    do { __builtin_printf ("Failure at line: %d\n", 293); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&q->a[2].c[2], 1) != sizeof (q->a[0].c) - 2)
    do { __builtin_printf ("Failure at line: %d\n", 295); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&q->a[3].a[2], 1) != sizeof (q->a[0].a) - 2)
    do { __builtin_printf ("Failure at line: %d\n", 297); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&q->a[3].c[2], 1) != sizeof (q->a[0].c) - 2)
    do { __builtin_printf ("Failure at line: %d\n", 299); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&q->a[i].a[2], 1) != sizeof (q->a[0].a) - 2)
    do { __builtin_printf ("Failure at line: %d\n", 301); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&q->a[i].c[2], 1) != sizeof (q->a[0].c) - 2)
    do { __builtin_printf ("Failure at line: %d\n", 303); nfails++; } while (0);
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
  if (__builtin_dynamic_object_size (&c->b, 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 317); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&c->b, 1) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 319); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&c->b, 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 321); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&c->b, 3) != 1)
    do { __builtin_printf ("Failure at line: %d\n", 323); nfails++; } while (0);
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
  if (__builtin_dynamic_object_size (&d->j.a[3], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&d->j.a[3], 1) != sizeof (d->j.a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&d->j.a[3], 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 345); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&d->j.a[3], 3) != sizeof (d->j.a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 347); nfails++; } while (0);
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
  if (__builtin_dynamic_object_size (&e->j[0].a[3], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 365); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&e->j[0].a[3], 1) != sizeof (e->j[0].a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 367); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&e->j[0].a[3], 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 369); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&e->j[0].a[3], 3) != sizeof (e->j[0].a) - 3)
    do { __builtin_printf ("Failure at line: %d\n", 371); nfails++; } while (0);
  if (__builtin_dynamic_object_size ((char *) &e->j[0], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 373); nfails++; } while (0);
  if (__builtin_dynamic_object_size ((char *) &e->j[0], 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 375); nfails++; } while (0);
  if (__builtin_dynamic_object_size ((char *) &e->j[0], 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 377); nfails++; } while (0);
  if (__builtin_dynamic_object_size ((char *) &e->j[0], 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 379); nfails++; } while (0);
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
  if (__builtin_dynamic_object_size (&f->d.c[3], 0) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 397); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&f->d.c[3], 1) != (size_t) -1)
    do { __builtin_printf ("Failure at line: %d\n", 399); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&f->d.c[3], 2) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 401); nfails++; } while (0);
  if (__builtin_dynamic_object_size (&f->d.c[3], 3) != 0)
    do { __builtin_printf ("Failure at line: %d\n", 403); nfails++; } while (0);
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
  do { if (nfails > 0) __builtin_abort (); return 0; } while (0);
}
# 6 "./ext/builtin-dynamic-object-size1.C" 2
