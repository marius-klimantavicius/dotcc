//type: rp
//options: 
# 0 "./builtin-dynamic-object-size-13.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtin-dynamic-object-size-13.c"




# 1 "./builtin-object-size-13.c" 1



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
# 5 "./builtin-object-size-13.c" 2

union A
{
  int a1;
  char a2[3];
};

union B
{
  long long b1;
  union A b2;
};

struct C
{
  int c1;
  union A c2;
};

struct D
{
  int d1;
  union B d2;
};

union E
{
  struct C e1;
  char e2[3];
};

union F
{
  int f1;
  struct D f2;
};

struct G
{
  union A g1;
  char g2;
};

struct H
{
  int h1;
  union E h2;
};
# 68 "./builtin-object-size-13.c"
int
main (void)
{
  size_t s, o, o2;

  s = sizeof (union A);
  o = 0;
  union A *a1 = malloc (s);
  union A *a2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&a1->a1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 77); nfails++; } while (0); if (__builtin_dynamic_object_size (&a1->a1, 1) != (sizeof (a1->a1))) do { __builtin_printf ("Failure at line: %d\n", 77); nfails++; } while (0); if (__builtin_dynamic_object_size (&a1->a1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 77); nfails++; } while (0); if (__builtin_dynamic_object_size (&a1->a1, 3) != (sizeof (a1->a1))) do { __builtin_printf ("Failure at line: %d\n", 77); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&a2->a1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 79); nfails++; } while (0); if (__builtin_dynamic_object_size (&a2->a1, 1) != (sizeof (a2->a1))) do { __builtin_printf ("Failure at line: %d\n", 79); nfails++; } while (0); if (__builtin_dynamic_object_size (&a2->a1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 79); nfails++; } while (0); if (__builtin_dynamic_object_size (&a2->a1, 3) != (sizeof (a2->a1))) do { __builtin_printf ("Failure at line: %d\n", 79); nfails++; } while (0);
  free (a2);
  free (a1);
  s = sizeof (union A);
  o = 0;
  a1 = malloc (s);
  a2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (a1->a2, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0); if (__builtin_dynamic_object_size (a1->a2, 1) != (s)) do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0); if (__builtin_dynamic_object_size (a1->a2, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0); if (__builtin_dynamic_object_size (a1->a2, 3) != (s)) do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0); if (__builtin_dynamic_object_size (&a1->a2[0], 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0); if (__builtin_dynamic_object_size (&a1->a2[0], 1) != (s)) do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0); if (__builtin_dynamic_object_size (&a1->a2[0], 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0); if (__builtin_dynamic_object_size (&a1->a2[0], 3) != (s)) do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0); if (__builtin_dynamic_object_size (&a1->a2[1], 0) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0); if (__builtin_dynamic_object_size (&a1->a2[1], 1) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0); if (__builtin_dynamic_object_size (&a1->a2[1], 2) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0); if (__builtin_dynamic_object_size (&a1->a2[1], 3) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 86); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (a2->a2, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 88); nfails++; } while (0); if (__builtin_dynamic_object_size (a2->a2, 1) != (s)) do { __builtin_printf ("Failure at line: %d\n", 88); nfails++; } while (0); if (__builtin_dynamic_object_size (a2->a2, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 88); nfails++; } while (0); if (__builtin_dynamic_object_size (a2->a2, 3) != (s)) do { __builtin_printf ("Failure at line: %d\n", 88); nfails++; } while (0); if (__builtin_dynamic_object_size (&a2->a2[0], 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 88); nfails++; } while (0); if (__builtin_dynamic_object_size (&a2->a2[0], 1) != (s)) do { __builtin_printf ("Failure at line: %d\n", 88); nfails++; } while (0); if (__builtin_dynamic_object_size (&a2->a2[0], 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 88); nfails++; } while (0); if (__builtin_dynamic_object_size (&a2->a2[0], 3) != (s)) do { __builtin_printf ("Failure at line: %d\n", 88); nfails++; } while (0); if (__builtin_dynamic_object_size (&a2->a2[1], 0) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 88); nfails++; } while (0); if (__builtin_dynamic_object_size (&a2->a2[1], 1) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 88); nfails++; } while (0); if (__builtin_dynamic_object_size (&a2->a2[1], 2) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 88); nfails++; } while (0); if (__builtin_dynamic_object_size (&a2->a2[1], 3) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 88); nfails++; } while (0);
  free (a2);
  free (a1);

  s = sizeof (union B);
  o = 0;
  union B *b1 = malloc (s);
  union B *b2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&b1->b1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 96); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b1, 1) != (sizeof (b1->b1))) do { __builtin_printf ("Failure at line: %d\n", 96); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 96); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b1, 3) != (sizeof (b1->b1))) do { __builtin_printf ("Failure at line: %d\n", 96); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&b2->b1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 98); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b1, 1) != (sizeof (b2->b1))) do { __builtin_printf ("Failure at line: %d\n", 98); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 98); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b1, 3) != (sizeof (b2->b1))) do { __builtin_printf ("Failure at line: %d\n", 98); nfails++; } while (0);
  free (b2);
  free (b1);
  s = sizeof (union B);
  o = 0;
  b1 = malloc (s);
  b2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&b1->b2.a1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 105); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b2.a1, 1) != (sizeof (b1->b2.a1))) do { __builtin_printf ("Failure at line: %d\n", 105); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b2.a1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 105); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b2.a1, 3) != (sizeof (b1->b2.a1))) do { __builtin_printf ("Failure at line: %d\n", 105); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&b2->b2.a1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 107); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b2.a1, 1) != (sizeof (b2->b2.a1))) do { __builtin_printf ("Failure at line: %d\n", 107); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b2.a1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 107); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b2.a1, 3) != (sizeof (b2->b2.a1))) do { __builtin_printf ("Failure at line: %d\n", 107); nfails++; } while (0);
  free (b2);
  free (b1);
  s = sizeof (union B);
  o = 0;
  b1 = malloc (s);
  b2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (b1->b2.a2, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0); if (__builtin_dynamic_object_size (b1->b2.a2, 1) != (s)) do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0); if (__builtin_dynamic_object_size (b1->b2.a2, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0); if (__builtin_dynamic_object_size (b1->b2.a2, 3) != (s)) do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b2.a2[0], 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b2.a2[0], 1) != (s)) do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b2.a2[0], 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b2.a2[0], 3) != (s)) do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b2.a2[1], 0) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b2.a2[1], 1) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b2.a2[1], 2) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0); if (__builtin_dynamic_object_size (&b1->b2.a2[1], 3) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 114); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (b2->b2.a2, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 116); nfails++; } while (0); if (__builtin_dynamic_object_size (b2->b2.a2, 1) != (s)) do { __builtin_printf ("Failure at line: %d\n", 116); nfails++; } while (0); if (__builtin_dynamic_object_size (b2->b2.a2, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 116); nfails++; } while (0); if (__builtin_dynamic_object_size (b2->b2.a2, 3) != (s)) do { __builtin_printf ("Failure at line: %d\n", 116); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b2.a2[0], 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 116); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b2.a2[0], 1) != (s)) do { __builtin_printf ("Failure at line: %d\n", 116); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b2.a2[0], 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 116); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b2.a2[0], 3) != (s)) do { __builtin_printf ("Failure at line: %d\n", 116); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b2.a2[1], 0) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 116); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b2.a2[1], 1) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 116); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b2.a2[1], 2) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 116); nfails++; } while (0); if (__builtin_dynamic_object_size (&b2->b2.a2[1], 3) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 116); nfails++; } while (0);
  free (b2);
  free (b1);

  s = sizeof (struct C);
  o = __builtin_offsetof (struct C, c2);
  struct C *c1 = malloc (s);
  struct C *c2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&c1->c1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 124); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c1, 1) != (sizeof (c1->c1))) do { __builtin_printf ("Failure at line: %d\n", 124); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 124); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c1, 3) != (sizeof (c1->c1))) do { __builtin_printf ("Failure at line: %d\n", 124); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&c2->c1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 126); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c1, 1) != (sizeof (c2->c1))) do { __builtin_printf ("Failure at line: %d\n", 126); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 126); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c1, 3) != (sizeof (c2->c1))) do { __builtin_printf ("Failure at line: %d\n", 126); nfails++; } while (0);
  free (c2);
  free (c1);
  s = sizeof (struct C);
  o = __builtin_offsetof (struct C, c2);
  c1 = malloc (s);
  c2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&c1->c2.a1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 133); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c2.a1, 1) != (sizeof (c1->c2.a1))) do { __builtin_printf ("Failure at line: %d\n", 133); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c2.a1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 133); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c2.a1, 3) != (sizeof (c1->c2.a1))) do { __builtin_printf ("Failure at line: %d\n", 133); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&c2->c2.a1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 135); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c2.a1, 1) != (sizeof (c2->c2.a1))) do { __builtin_printf ("Failure at line: %d\n", 135); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c2.a1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 135); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c2.a1, 3) != (sizeof (c2->c2.a1))) do { __builtin_printf ("Failure at line: %d\n", 135); nfails++; } while (0);
  free (c2);
  free (c1);
  s = sizeof (struct C);
  o = __builtin_offsetof (struct C, c2);
  c1 = malloc (s);
  c2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (c1->c2.a2, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0); if (__builtin_dynamic_object_size (c1->c2.a2, 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0); if (__builtin_dynamic_object_size (c1->c2.a2, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0); if (__builtin_dynamic_object_size (c1->c2.a2, 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c2.a2[0], 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c2.a2[0], 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c2.a2[0], 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c2.a2[0], 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c2.a2[1], 0) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c2.a2[1], 1) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c2.a2[1], 2) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0); if (__builtin_dynamic_object_size (&c1->c2.a2[1], 3) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 142); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (c2->c2.a2, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0); if (__builtin_dynamic_object_size (c2->c2.a2, 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0); if (__builtin_dynamic_object_size (c2->c2.a2, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0); if (__builtin_dynamic_object_size (c2->c2.a2, 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c2.a2[0], 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c2.a2[0], 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c2.a2[0], 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c2.a2[0], 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c2.a2[1], 0) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c2.a2[1], 1) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c2.a2[1], 2) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0); if (__builtin_dynamic_object_size (&c2->c2.a2[1], 3) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 144); nfails++; } while (0);
  free (c2);
  free (c1);

  s = sizeof (struct D);
  o = __builtin_offsetof (struct D, d2);
  struct D *d1 = malloc (s);
  struct D *d2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&d1->d1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 152); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d1, 1) != (sizeof (d1->d1))) do { __builtin_printf ("Failure at line: %d\n", 152); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 152); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d1, 3) != (sizeof (d1->d1))) do { __builtin_printf ("Failure at line: %d\n", 152); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&d2->d1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 154); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d1, 1) != (sizeof (d2->d1))) do { __builtin_printf ("Failure at line: %d\n", 154); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 154); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d1, 3) != (sizeof (d2->d1))) do { __builtin_printf ("Failure at line: %d\n", 154); nfails++; } while (0);
  free (d2);
  free (d1);
  s = sizeof (struct D);
  o = __builtin_offsetof (struct D, d2);
  d1 = malloc (s);
  d2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&d1->d2.b1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 161); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b1, 1) != (sizeof (d1->d2.b1))) do { __builtin_printf ("Failure at line: %d\n", 161); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 161); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b1, 3) != (sizeof (d1->d2.b1))) do { __builtin_printf ("Failure at line: %d\n", 161); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&d2->d2.b1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 163); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b1, 1) != (sizeof (d2->d2.b1))) do { __builtin_printf ("Failure at line: %d\n", 163); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 163); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b1, 3) != (sizeof (d2->d2.b1))) do { __builtin_printf ("Failure at line: %d\n", 163); nfails++; } while (0);
  free (d2);
  free (d1);
  s = sizeof (struct D);
  o = __builtin_offsetof (struct D, d2);
  d1 = malloc (s);
  d2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&d1->d2.b2.a1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 170); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b2.a1, 1) != (sizeof (d1->d2.b2.a1))) do { __builtin_printf ("Failure at line: %d\n", 170); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b2.a1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 170); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b2.a1, 3) != (sizeof (d1->d2.b2.a1))) do { __builtin_printf ("Failure at line: %d\n", 170); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&d2->d2.b2.a1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 172); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b2.a1, 1) != (sizeof (d2->d2.b2.a1))) do { __builtin_printf ("Failure at line: %d\n", 172); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b2.a1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 172); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b2.a1, 3) != (sizeof (d2->d2.b2.a1))) do { __builtin_printf ("Failure at line: %d\n", 172); nfails++; } while (0);
  free (d2);
  free (d1);
  s = sizeof (struct D);
  o = __builtin_offsetof (struct D, d2);
  d1 = malloc (s);
  d2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (d1->d2.b2.a2, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0); if (__builtin_dynamic_object_size (d1->d2.b2.a2, 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0); if (__builtin_dynamic_object_size (d1->d2.b2.a2, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0); if (__builtin_dynamic_object_size (d1->d2.b2.a2, 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b2.a2[0], 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b2.a2[0], 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b2.a2[0], 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b2.a2[0], 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b2.a2[1], 0) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b2.a2[1], 1) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b2.a2[1], 2) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0); if (__builtin_dynamic_object_size (&d1->d2.b2.a2[1], 3) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 179); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (d2->d2.b2.a2, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0); if (__builtin_dynamic_object_size (d2->d2.b2.a2, 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0); if (__builtin_dynamic_object_size (d2->d2.b2.a2, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0); if (__builtin_dynamic_object_size (d2->d2.b2.a2, 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b2.a2[0], 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b2.a2[0], 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b2.a2[0], 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b2.a2[0], 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b2.a2[1], 0) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b2.a2[1], 1) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b2.a2[1], 2) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0); if (__builtin_dynamic_object_size (&d2->d2.b2.a2[1], 3) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 181); nfails++; } while (0);
  free (d2);
  free (d1);

  s = sizeof (union E);
  o = __builtin_offsetof (union E, e1.c2);
  union E *e1 = malloc (s);
  union E *e2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&e1->e1.c1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 189); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c1, 1) != (sizeof (e1->e1.c1))) do { __builtin_printf ("Failure at line: %d\n", 189); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 189); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c1, 3) != (sizeof (e1->e1.c1))) do { __builtin_printf ("Failure at line: %d\n", 189); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&e2->e1.c1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 191); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c1, 1) != (sizeof (e2->e1.c1))) do { __builtin_printf ("Failure at line: %d\n", 191); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 191); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c1, 3) != (sizeof (e2->e1.c1))) do { __builtin_printf ("Failure at line: %d\n", 191); nfails++; } while (0);
  free (e2);
  free (e1);
  s = sizeof (union E);
  o = __builtin_offsetof (union E, e1.c2);
  e1 = malloc (s);
  e2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&e1->e1.c2.a1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 198); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c2.a1, 1) != (sizeof (e1->e1.c2.a1))) do { __builtin_printf ("Failure at line: %d\n", 198); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c2.a1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 198); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c2.a1, 3) != (sizeof (e1->e1.c2.a1))) do { __builtin_printf ("Failure at line: %d\n", 198); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&e2->e1.c2.a1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 200); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c2.a1, 1) != (sizeof (e2->e1.c2.a1))) do { __builtin_printf ("Failure at line: %d\n", 200); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c2.a1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 200); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c2.a1, 3) != (sizeof (e2->e1.c2.a1))) do { __builtin_printf ("Failure at line: %d\n", 200); nfails++; } while (0);
  free (e2);
  free (e1);
  s = sizeof (union E);
  o = __builtin_offsetof (union E, e1.c2);
  e1 = malloc (s);
  e2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (e1->e1.c2.a2, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0); if (__builtin_dynamic_object_size (e1->e1.c2.a2, 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0); if (__builtin_dynamic_object_size (e1->e1.c2.a2, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0); if (__builtin_dynamic_object_size (e1->e1.c2.a2, 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c2.a2[0], 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c2.a2[0], 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c2.a2[0], 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c2.a2[0], 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c2.a2[1], 0) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c2.a2[1], 1) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c2.a2[1], 2) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e1.c2.a2[1], 3) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 207); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (e2->e1.c2.a2, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0); if (__builtin_dynamic_object_size (e2->e1.c2.a2, 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0); if (__builtin_dynamic_object_size (e2->e1.c2.a2, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0); if (__builtin_dynamic_object_size (e2->e1.c2.a2, 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c2.a2[0], 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c2.a2[0], 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c2.a2[0], 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c2.a2[0], 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c2.a2[1], 0) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c2.a2[1], 1) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c2.a2[1], 2) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e1.c2.a2[1], 3) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 209); nfails++; } while (0);
  free (e2);
  free (e1);
  s = sizeof (union E);
  o = __builtin_offsetof (union E, e1.c2);
  e1 = malloc (s);
  e2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (e1->e2, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0); if (__builtin_dynamic_object_size (e1->e2, 1) != (s)) do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0); if (__builtin_dynamic_object_size (e1->e2, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0); if (__builtin_dynamic_object_size (e1->e2, 3) != (s)) do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e2[0], 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e2[0], 1) != (s)) do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e2[0], 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e2[0], 3) != (s)) do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e2[1], 0) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e2[1], 1) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e2[1], 2) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0); if (__builtin_dynamic_object_size (&e1->e2[1], 3) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 216); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (e2->e2, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0); if (__builtin_dynamic_object_size (e2->e2, 1) != (s)) do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0); if (__builtin_dynamic_object_size (e2->e2, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0); if (__builtin_dynamic_object_size (e2->e2, 3) != (s)) do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e2[0], 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e2[0], 1) != (s)) do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e2[0], 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e2[0], 3) != (s)) do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e2[1], 0) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e2[1], 1) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e2[1], 2) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0); if (__builtin_dynamic_object_size (&e2->e2[1], 3) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 218); nfails++; } while (0);
  free (e2);
  free (e1);

  s = sizeof (union F);
  o = __builtin_offsetof (union F, f2.d2);
  union F *f1 = malloc (s);
  union F *f2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&f1->f1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 226); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f1, 1) != (sizeof (f1->f1))) do { __builtin_printf ("Failure at line: %d\n", 226); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 226); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f1, 3) != (sizeof (f1->f1))) do { __builtin_printf ("Failure at line: %d\n", 226); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&f2->f1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 228); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f1, 1) != (sizeof (f2->f1))) do { __builtin_printf ("Failure at line: %d\n", 228); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 228); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f1, 3) != (sizeof (f2->f1))) do { __builtin_printf ("Failure at line: %d\n", 228); nfails++; } while (0);
  free (f2);
  free (f1);
  s = sizeof (union F);
  o = __builtin_offsetof (union F, f2.d2);
  f1 = malloc (s);
  f2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&f1->f2.d1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 235); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d1, 1) != (sizeof (f1->f2.d1))) do { __builtin_printf ("Failure at line: %d\n", 235); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 235); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d1, 3) != (sizeof (f1->f2.d1))) do { __builtin_printf ("Failure at line: %d\n", 235); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&f2->f2.d1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 237); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d1, 1) != (sizeof (f2->f2.d1))) do { __builtin_printf ("Failure at line: %d\n", 237); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 237); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d1, 3) != (sizeof (f2->f2.d1))) do { __builtin_printf ("Failure at line: %d\n", 237); nfails++; } while (0);
  free (f2);
  free (f1);
  s = sizeof (union F);
  o = __builtin_offsetof (union F, f2.d2);
  f1 = malloc (s);
  f2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&f1->f2.d2.b1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 244); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b1, 1) != (sizeof (f1->f2.d2.b1))) do { __builtin_printf ("Failure at line: %d\n", 244); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 244); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b1, 3) != (sizeof (f1->f2.d2.b1))) do { __builtin_printf ("Failure at line: %d\n", 244); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&f2->f2.d2.b1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 246); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b1, 1) != (sizeof (f2->f2.d2.b1))) do { __builtin_printf ("Failure at line: %d\n", 246); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 246); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b1, 3) != (sizeof (f2->f2.d2.b1))) do { __builtin_printf ("Failure at line: %d\n", 246); nfails++; } while (0);
  free (f2);
  free (f1);
  s = sizeof (union F);
  o = __builtin_offsetof (union F, f2.d2);
  f1 = malloc (s);
  f2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&f1->f2.d2.b2.a1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 253); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b2.a1, 1) != (sizeof (f1->f2.d2.b2.a1))) do { __builtin_printf ("Failure at line: %d\n", 253); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b2.a1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 253); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b2.a1, 3) != (sizeof (f1->f2.d2.b2.a1))) do { __builtin_printf ("Failure at line: %d\n", 253); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&f2->f2.d2.b2.a1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 255); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b2.a1, 1) != (sizeof (f2->f2.d2.b2.a1))) do { __builtin_printf ("Failure at line: %d\n", 255); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b2.a1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 255); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b2.a1, 3) != (sizeof (f2->f2.d2.b2.a1))) do { __builtin_printf ("Failure at line: %d\n", 255); nfails++; } while (0);
  free (f2);
  free (f1);
  s = sizeof (union F);
  o = __builtin_offsetof (union F, f2.d2);
  f1 = malloc (s);
  f2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (f1->f2.d2.b2.a2, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 262); nfails++; } while (0); if (__builtin_dynamic_object_size (f1->f2.d2.b2.a2, 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 262); nfails++; } while (0); if (__builtin_dynamic_object_size (f1->f2.d2.b2.a2, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 262); nfails++; } while (0); if (__builtin_dynamic_object_size (f1->f2.d2.b2.a2, 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 262); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b2.a2[0], 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 262); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b2.a2[0], 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 262); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b2.a2[0], 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 262); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b2.a2[0], 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 262); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b2.a2[1], 0) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 262); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b2.a2[1], 1) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 262); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b2.a2[1], 2) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 262); nfails++; } while (0); if (__builtin_dynamic_object_size (&f1->f2.d2.b2.a2[1], 3) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 262); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (f2->f2.d2.b2.a2, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0); if (__builtin_dynamic_object_size (f2->f2.d2.b2.a2, 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0); if (__builtin_dynamic_object_size (f2->f2.d2.b2.a2, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0); if (__builtin_dynamic_object_size (f2->f2.d2.b2.a2, 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b2.a2[0], 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b2.a2[0], 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b2.a2[0], 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b2.a2[0], 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b2.a2[1], 0) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b2.a2[1], 1) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b2.a2[1], 2) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0); if (__builtin_dynamic_object_size (&f2->f2.d2.b2.a2[1], 3) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 264); nfails++; } while (0);
  free (f2);
  free (f1);

  s = sizeof (struct G);
  o = __builtin_offsetof (struct G, g2);
  struct G *g1 = malloc (s);
  struct G *g2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&g1->g1.a1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 272); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g1.a1, 1) != (sizeof (g1->g1.a1))) do { __builtin_printf ("Failure at line: %d\n", 272); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g1.a1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 272); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g1.a1, 3) != (sizeof (g1->g1.a1))) do { __builtin_printf ("Failure at line: %d\n", 272); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&g2->g1.a1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 274); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g1.a1, 1) != (sizeof (g2->g1.a1))) do { __builtin_printf ("Failure at line: %d\n", 274); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g1.a1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 274); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g1.a1, 3) != (sizeof (g2->g1.a1))) do { __builtin_printf ("Failure at line: %d\n", 274); nfails++; } while (0);
  free (g2);
  free (g1);
  s = sizeof (struct G);
  o = __builtin_offsetof (struct G, g2);
  g1 = malloc (s);
  g2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (g1->g1.a2, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 281); nfails++; } while (0); if (__builtin_dynamic_object_size (g1->g1.a2, 1) != (sizeof (g1->g1.a2))) do { __builtin_printf ("Failure at line: %d\n", 281); nfails++; } while (0); if (__builtin_dynamic_object_size (g1->g1.a2, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 281); nfails++; } while (0); if (__builtin_dynamic_object_size (g1->g1.a2, 3) != (sizeof (g1->g1.a2))) do { __builtin_printf ("Failure at line: %d\n", 281); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g1.a2[0], 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 281); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g1.a2[0], 1) != (sizeof (g1->g1.a2))) do { __builtin_printf ("Failure at line: %d\n", 281); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g1.a2[0], 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 281); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g1.a2[0], 3) != (sizeof (g1->g1.a2))) do { __builtin_printf ("Failure at line: %d\n", 281); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g1.a2[1], 0) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 281); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g1.a2[1], 1) != ((sizeof (g1->g1.a2)) - 1)) do { __builtin_printf ("Failure at line: %d\n", 281); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g1.a2[1], 2) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 281); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g1.a2[1], 3) != ((sizeof (g1->g1.a2)) - 1)) do { __builtin_printf ("Failure at line: %d\n", 281); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (g2->g1.a2, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 283); nfails++; } while (0); if (__builtin_dynamic_object_size (g2->g1.a2, 1) != (sizeof (g1->g1.a2))) do { __builtin_printf ("Failure at line: %d\n", 283); nfails++; } while (0); if (__builtin_dynamic_object_size (g2->g1.a2, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 283); nfails++; } while (0); if (__builtin_dynamic_object_size (g2->g1.a2, 3) != (sizeof (g1->g1.a2))) do { __builtin_printf ("Failure at line: %d\n", 283); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g1.a2[0], 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 283); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g1.a2[0], 1) != (sizeof (g1->g1.a2))) do { __builtin_printf ("Failure at line: %d\n", 283); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g1.a2[0], 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 283); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g1.a2[0], 3) != (sizeof (g1->g1.a2))) do { __builtin_printf ("Failure at line: %d\n", 283); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g1.a2[1], 0) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 283); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g1.a2[1], 1) != ((sizeof (g1->g1.a2)) - 1)) do { __builtin_printf ("Failure at line: %d\n", 283); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g1.a2[1], 2) != ((s) - 1)) do { __builtin_printf ("Failure at line: %d\n", 283); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g1.a2[1], 3) != ((sizeof (g1->g1.a2)) - 1)) do { __builtin_printf ("Failure at line: %d\n", 283); nfails++; } while (0);
  free (g2);
  free (g1);
  s = sizeof (struct G);
  o = __builtin_offsetof (struct G, g2);
  g1 = malloc (s);
  g2 = malloc (o + 212);
  if (__builtin_dynamic_object_size (&g1->g2, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 290); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g2, 1) != (sizeof (g1->g2))) do { __builtin_printf ("Failure at line: %d\n", 290); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g2, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 290); nfails++; } while (0); if (__builtin_dynamic_object_size (&g1->g2, 3) != (sizeof (g1->g2))) do { __builtin_printf ("Failure at line: %d\n", 290); nfails++; } while (0);
  s = o + 212;
  if (__builtin_dynamic_object_size (&g2->g2, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 292); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g2, 1) != (sizeof (g2->g2))) do { __builtin_printf ("Failure at line: %d\n", 292); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g2, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 292); nfails++; } while (0); if (__builtin_dynamic_object_size (&g2->g2, 3) != (sizeof (g2->g2))) do { __builtin_printf ("Failure at line: %d\n", 292); nfails++; } while (0);
  free (g2);
  free (g1);

  s = sizeof (struct H);
  o = __builtin_offsetof (struct H, h2);
  o2 = __builtin_offsetof (struct H, h2.e1.c2);
  struct H *h1 = malloc (s);
  struct H *h2 = malloc (o2 + 212);
  if (__builtin_dynamic_object_size (&h1->h1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 301); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h1, 1) != (sizeof (h1->h1))) do { __builtin_printf ("Failure at line: %d\n", 301); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 301); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h1, 3) != (sizeof (h1->h1))) do { __builtin_printf ("Failure at line: %d\n", 301); nfails++; } while (0);
  s = o2 + 212;
  if (__builtin_dynamic_object_size (&h2->h1, 0) != (s)) do { __builtin_printf ("Failure at line: %d\n", 303); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h1, 1) != (sizeof (h2->h1))) do { __builtin_printf ("Failure at line: %d\n", 303); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h1, 2) != (s)) do { __builtin_printf ("Failure at line: %d\n", 303); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h1, 3) != (sizeof (h2->h1))) do { __builtin_printf ("Failure at line: %d\n", 303); nfails++; } while (0);
  free (h2);
  free (h1);
  s = sizeof (struct H);
  o = __builtin_offsetof (struct H, h2);
  o2 = __builtin_offsetof (struct H, h2.e1.c2);
  h1 = malloc (s);
  h2 = malloc (o2 + 212);
  if (__builtin_dynamic_object_size (&h1->h2.e1.c1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 311); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c1, 1) != (sizeof (h1->h2.e1.c1))) do { __builtin_printf ("Failure at line: %d\n", 311); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 311); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c1, 3) != (sizeof (h1->h2.e1.c1))) do { __builtin_printf ("Failure at line: %d\n", 311); nfails++; } while (0);
  s = o2 + 212;
  if (__builtin_dynamic_object_size (&h2->h2.e1.c1, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 313); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c1, 1) != (sizeof (h2->h2.e1.c1))) do { __builtin_printf ("Failure at line: %d\n", 313); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c1, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 313); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c1, 3) != (sizeof (h2->h2.e1.c1))) do { __builtin_printf ("Failure at line: %d\n", 313); nfails++; } while (0);
  free (h2);
  free (h1);
  s = sizeof (struct H);
  o = __builtin_offsetof (struct H, h2);
  o2 = __builtin_offsetof (struct H, h2.e1.c2);
  h1 = malloc (s);
  h2 = malloc (o2 + 212);
  if (__builtin_dynamic_object_size (&h1->h2.e1.c2.a1, 0) != (s - o2)) do { __builtin_printf ("Failure at line: %d\n", 321); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c2.a1, 1) != (sizeof (h1->h2.e1.c2.a1))) do { __builtin_printf ("Failure at line: %d\n", 321); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c2.a1, 2) != (s - o2)) do { __builtin_printf ("Failure at line: %d\n", 321); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c2.a1, 3) != (sizeof (h1->h2.e1.c2.a1))) do { __builtin_printf ("Failure at line: %d\n", 321); nfails++; } while (0);
  s = o2 + 212;
  if (__builtin_dynamic_object_size (&h2->h2.e1.c2.a1, 0) != (s - o2)) do { __builtin_printf ("Failure at line: %d\n", 323); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c2.a1, 1) != (sizeof (h2->h2.e1.c2.a1))) do { __builtin_printf ("Failure at line: %d\n", 323); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c2.a1, 2) != (s - o2)) do { __builtin_printf ("Failure at line: %d\n", 323); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c2.a1, 3) != (sizeof (h2->h2.e1.c2.a1))) do { __builtin_printf ("Failure at line: %d\n", 323); nfails++; } while (0);
  free (h2);
  free (h1);
  s = sizeof (struct H);
  o = __builtin_offsetof (struct H, h2);
  o2 = __builtin_offsetof (struct H, h2.e1.c2);
  h1 = malloc (s);
  h2 = malloc (o2 + 212);
  if (__builtin_dynamic_object_size (h1->h2.e1.c2.a2, 0) != (s - o2)) do { __builtin_printf ("Failure at line: %d\n", 331); nfails++; } while (0); if (__builtin_dynamic_object_size (h1->h2.e1.c2.a2, 1) != (sizeof (h1->h2.e1.c2.a2))) do { __builtin_printf ("Failure at line: %d\n", 331); nfails++; } while (0); if (__builtin_dynamic_object_size (h1->h2.e1.c2.a2, 2) != (s - o2)) do { __builtin_printf ("Failure at line: %d\n", 331); nfails++; } while (0); if (__builtin_dynamic_object_size (h1->h2.e1.c2.a2, 3) != (sizeof (h1->h2.e1.c2.a2))) do { __builtin_printf ("Failure at line: %d\n", 331); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c2.a2[0], 0) != (s - o2)) do { __builtin_printf ("Failure at line: %d\n", 331); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c2.a2[0], 1) != (sizeof (h1->h2.e1.c2.a2))) do { __builtin_printf ("Failure at line: %d\n", 331); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c2.a2[0], 2) != (s - o2)) do { __builtin_printf ("Failure at line: %d\n", 331); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c2.a2[0], 3) != (sizeof (h1->h2.e1.c2.a2))) do { __builtin_printf ("Failure at line: %d\n", 331); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c2.a2[1], 0) != ((s - o2) - 1)) do { __builtin_printf ("Failure at line: %d\n", 331); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c2.a2[1], 1) != ((sizeof (h1->h2.e1.c2.a2)) - 1)) do { __builtin_printf ("Failure at line: %d\n", 331); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c2.a2[1], 2) != ((s - o2) - 1)) do { __builtin_printf ("Failure at line: %d\n", 331); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e1.c2.a2[1], 3) != ((sizeof (h1->h2.e1.c2.a2)) - 1)) do { __builtin_printf ("Failure at line: %d\n", 331); nfails++; } while (0);
  s = o2 + 212;
  if (__builtin_dynamic_object_size (h2->h2.e1.c2.a2, 0) != (s - o2)) do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0); if (__builtin_dynamic_object_size (h2->h2.e1.c2.a2, 1) != (sizeof (h2->h2.e1.c2.a2))) do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0); if (__builtin_dynamic_object_size (h2->h2.e1.c2.a2, 2) != (s - o2)) do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0); if (__builtin_dynamic_object_size (h2->h2.e1.c2.a2, 3) != (sizeof (h2->h2.e1.c2.a2))) do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c2.a2[0], 0) != (s - o2)) do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c2.a2[0], 1) != (sizeof (h2->h2.e1.c2.a2))) do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c2.a2[0], 2) != (s - o2)) do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c2.a2[0], 3) != (sizeof (h2->h2.e1.c2.a2))) do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c2.a2[1], 0) != ((s - o2) - 1)) do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c2.a2[1], 1) != ((sizeof (h2->h2.e1.c2.a2)) - 1)) do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c2.a2[1], 2) != ((s - o2) - 1)) do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e1.c2.a2[1], 3) != ((sizeof (h2->h2.e1.c2.a2)) - 1)) do { __builtin_printf ("Failure at line: %d\n", 333); nfails++; } while (0);
  free (h2);
  free (h1);
  s = sizeof (struct H);
  o = __builtin_offsetof (struct H, h2);
  o2 = __builtin_offsetof (struct H, h2.e1.c2);
  h1 = malloc (s);
  h2 = malloc (o2 + 212);
  if (__builtin_dynamic_object_size (h1->h2.e2, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0); if (__builtin_dynamic_object_size (h1->h2.e2, 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0); if (__builtin_dynamic_object_size (h1->h2.e2, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0); if (__builtin_dynamic_object_size (h1->h2.e2, 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e2[0], 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e2[0], 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e2[0], 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e2[0], 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e2[1], 0) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e2[1], 1) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e2[1], 2) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0); if (__builtin_dynamic_object_size (&h1->h2.e2[1], 3) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 341); nfails++; } while (0);
  s = o2 + 212;
  if (__builtin_dynamic_object_size (h2->h2.e2, 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0); if (__builtin_dynamic_object_size (h2->h2.e2, 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0); if (__builtin_dynamic_object_size (h2->h2.e2, 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0); if (__builtin_dynamic_object_size (h2->h2.e2, 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e2[0], 0) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e2[0], 1) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e2[0], 2) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e2[0], 3) != (s - o)) do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e2[1], 0) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e2[1], 1) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e2[1], 2) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0); if (__builtin_dynamic_object_size (&h2->h2.e2[1], 3) != ((s - o) - 1)) do { __builtin_printf ("Failure at line: %d\n", 343); nfails++; } while (0);
  free (h2);
  free (h1);

  do { if (nfails > 0) __builtin_abort (); return 0; } while (0);
}
# 6 "./builtin-dynamic-object-size-13.c" 2
