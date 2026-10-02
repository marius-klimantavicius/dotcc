//type: rp
//options: 
# 0 "./tree-ssa/sra-longjmp-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/sra-longjmp-1.c"




# 1 "/usr/include/setjmp.h" 1 3 4
# 25 "/usr/include/setjmp.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 26 "/usr/include/setjmp.h" 2 3 4



# 1 "/usr/include/bits/setjmp.h" 1 3 4
# 26 "/usr/include/bits/setjmp.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 27 "/usr/include/bits/setjmp.h" 2 3 4





# 31 "/usr/include/bits/setjmp.h" 3 4
typedef long int __jmp_buf[8];
# 30 "/usr/include/setjmp.h" 2 3 4
# 1 "/usr/include/bits/sigset.h" 1 3 4
# 23 "/usr/include/bits/sigset.h" 3 4
typedef int __sig_atomic_t;




typedef struct
  {
    unsigned long int __val[(1024 / (8 * sizeof (unsigned long int)))];
  } __sigset_t;
# 31 "/usr/include/setjmp.h" 2 3 4



struct __jmp_buf_tag
  {




    __jmp_buf __jmpbuf;
    int __mask_was_saved;
    __sigset_t __saved_mask;
  };




typedef struct __jmp_buf_tag jmp_buf[1];



extern int setjmp (jmp_buf __env) __attribute__ ((__nothrow__));






extern int __sigsetjmp (struct __jmp_buf_tag __env[1], int __savemask) __attribute__ ((__nothrow__));




extern int _setjmp (struct __jmp_buf_tag __env[1]) __attribute__ ((__nothrow__));
# 77 "/usr/include/setjmp.h" 3 4




extern void longjmp (struct __jmp_buf_tag __env[1], int __val)
     __attribute__ ((__nothrow__)) __attribute__ ((__noreturn__));







extern void _longjmp (struct __jmp_buf_tag __env[1], int __val)
     __attribute__ ((__nothrow__)) __attribute__ ((__noreturn__));







typedef struct __jmp_buf_tag sigjmp_buf[1];
# 109 "/usr/include/setjmp.h" 3 4
extern void siglongjmp (sigjmp_buf __env, int __val)
     __attribute__ ((__nothrow__)) __attribute__ ((__noreturn__));
# 119 "/usr/include/setjmp.h" 3 4

# 6 "./tree-ssa/sra-longjmp-1.c" 2


# 7 "./tree-ssa/sra-longjmp-1.c"
struct S {
  long *b;
  int c;
  int s;
};

static jmp_buf the_jmpbuf;
volatile short vs = 0;
long buf[16];
long * volatile pbuf = (long *) &buf;

static void __attribute__((noinline))
crazy_alloc_s (struct S *p)
{
  int s = p->c ? p->c * 2 : 16;

  long *b = pbuf;
  if (!b || s > 16)
    {
      p->s = -p->s;
      vs = 127;
      longjmp (the_jmpbuf, 1);
    }

  __builtin_memcpy (b, p->b, p->c);
  p->b = b;
  p->s = s;
  pbuf = 0;
  return;
}

long __attribute__((noipa))
process (long v)
{
  return v + 1;
}

void
foo (void)
{
  struct S stack;

  if (
# 49 "./tree-ssa/sra-longjmp-1.c" 3 4
     _setjmp (
# 49 "./tree-ssa/sra-longjmp-1.c"
     the_jmpbuf
# 49 "./tree-ssa/sra-longjmp-1.c" 3 4
     )
# 49 "./tree-ssa/sra-longjmp-1.c"
                        )
    return;

  stack.c = 0;
  crazy_alloc_s (&stack);
  stack.b[0] = 1;
  stack.c = 1;

  while (stack.c)
    {
      long l = stack.b[--stack.c];

      if (l > 0)
 {
   for (int i = 0; i < 4; i++)
     {
       if (stack.s <= stack.c + 1)
  crazy_alloc_s (&stack);
       l = process (l);
       stack.b[stack.c++] = l;
     }
 }
    }

  return;
}

int main (int argc, char **argv)
{
  vs = 0;
  pbuf = (long *) &buf;
  foo ();
  if (vs != 127)
    __builtin_abort ();

  return 0;
}
