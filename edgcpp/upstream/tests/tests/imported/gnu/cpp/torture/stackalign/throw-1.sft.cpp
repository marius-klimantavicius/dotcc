//type: rp
//options: 
# 0 "./torture/stackalign/throw-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/stackalign/throw-1.C"



# 1 "./torture/stackalign/check.h" 1
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 425 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
} max_align_t;






  typedef decltype(nullptr) nullptr_t;
# 2 "./torture/stackalign/check.h" 2






# 7 "./torture/stackalign/check.h"
extern "C" void abort (void);




int
check_int (int *i, int align)
{
  *i = 20;
  if ((((ptrdiff_t) i) & (align - 1)) != 0)
    {



      abort ();
    }
  return *i;
}

void
check (void *p, int align)
{
  if ((((ptrdiff_t) p) & (align - 1)) != 0)
    {



      abort ();
    }
}
# 5 "./torture/stackalign/throw-1.C" 2





typedef int t_align __attribute__((aligned(64)));


int global, global2;
void bar()
{
 volatile t_align a = 1;
        int i,j,k,l,m,n;
        i=j=k=0;
   for (i=0; i < global; i++)
   for (j=0; j < i; j++)
   for (k=0; k < j; k++)
   for (l=0; l < k; l++)
   for (m=0; m < l; m++)
   for (n=0; n < m; n++)
       global2 = k;
 if (check_int ((int *) &a, __alignof__(a)) != a)
   abort ();
 throw 0;
}

void foo()
{
 bar();
}

int main()
{
 int ll = 1;
        int i = 0,j = 1,k = 2,l = 3,m = 4,n = 5;
 try {
     for (; i < global; i++)
   for (; j < i; j++)
   for (; k < j; k++)
   for (; l < k; l++)
   for (; m < l; m++)
   for (; n < m; n++)
       global2 = k;
   foo();
 }
 catch (...)
 {
 }
 ll = i+j+k+l+m+n;
 if (ll != 15)
 {



  abort();
 }
 return 0;
}
