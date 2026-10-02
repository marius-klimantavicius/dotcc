//type: rp
//options: 
# 0 "./pr84503-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr84503-2.c"




# 1 "./pr84503-1.c" 1




typedef long unsigned int size_t;
typedef long unsigned int uintptr_t;

struct S { int a; unsigned short b; int c, d, e; long f, g, h; int i, j; };
static struct S *k;
static size_t l = 0;
int m;

static int
bar (void)
{
  unsigned i;
  int j;
  if (k[0].c == 0)
    {
      ++m;
      size_t n = l * 2;
      struct S *o;
      o = (struct S *) __builtin_realloc (k, sizeof (struct S) * n);
      if (!o)
 __builtin_exit (0);
      k = o;
      for (i = l; i < n; i++)
 {
   void *p = (void *) &k[i];
   int q = 0;
   size_t r = sizeof (struct S);
   if ((((uintptr_t) p) % __alignof__ (long)) == 0
       && r % sizeof (long) == 0)
     {
       long __attribute__ ((may_alias)) *s = (long *) p;
       long *t = (long *) ((char *) s + r);
       while (s < t)
  *s++ = 0;
     }
   else
     __builtin_memset (p, q, r);
   k[i].c = i + 1;
   k[i].a = -1;
 }
      k[n - 1].c = 0;
      k[0].c = l;
      l = n;
    }
  j = k[0].c;
  k[0].c = k[j].c;
  return j;
}

int
main ()
{
  k = (struct S *) __builtin_malloc (sizeof (struct S));
  if (!k)
    __builtin_exit (0);
  __builtin_memset (k, '\0', sizeof (struct S));
  k->a = -1;
  l = 1;
  for (int i = 0; i < 15; ++i)
    bar ();
  if (m != 4)
    __builtin_abort ();
  return 0;
}
# 6 "./pr84503-2.c" 2
