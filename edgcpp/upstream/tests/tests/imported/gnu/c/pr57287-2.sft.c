//type: fp
//options: 
# 0 "./pr57287-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr57287-2.c"




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

# 6 "./pr57287-2.c" 2


# 7 "./pr57287-2.c"
struct node
{
  struct node *next;
  char *name;
} *list;

struct node *list;
struct node *head (void);

jmp_buf *bar (void);

int baz (void)
{
  struct node *n;
  int varseen = 0;

  list = head ();
  for (n = list; n; n = n->next)
    {
      if (!varseen)
 varseen = 1;

      jmp_buf *buf = bar ();
      
# 30 "./pr57287-2.c" 3 4
     _setjmp (
# 30 "./pr57287-2.c"
     *buf
# 30 "./pr57287-2.c" 3 4
     )
# 30 "./pr57287-2.c"
                  ;
    }

  if (!varseen)
    return 0;
  return 1;
}
