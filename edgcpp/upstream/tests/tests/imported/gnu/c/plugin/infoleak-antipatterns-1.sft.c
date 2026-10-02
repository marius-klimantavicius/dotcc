//type: fp
//options: 
# 0 "./plugin/infoleak-antipatterns-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./plugin/infoleak-antipatterns-1.c"







typedef unsigned char u8;
typedef unsigned short int u16;
typedef unsigned int u32;
typedef long unsigned int size_t;



# 1 "./plugin/test-uaccess.h" 1
# 9 "./plugin/test-uaccess.h"
extern long copy_from_user(void *to, const void *from, long n);
extern long copy_to_user(void *to, const void *from, long n);
# 16 "./plugin/infoleak-antipatterns-1.c" 2

typedef unsigned int gfp_t;


void kfree(const void *);
void *kmalloc(size_t size, gfp_t flags)
  __attribute__((malloc (kfree)));



struct infoleak_buf
{
  char buf[256];
};

int infoleak_stack_no_init(void *dst)
{
  struct infoleak_buf st;



  if (copy_to_user(dst, &st, sizeof(st)))

    return -14;
  return 0;
}

int infoleak_heap_no_init(void *dst)
{
  struct infoleak_buf *heapbuf = kmalloc(sizeof(*heapbuf), 0);




  if (copy_to_user(dst, heapbuf, sizeof(*heapbuf)))

    return -14;

  kfree(heapbuf);
  return 0;
}

struct infoleak_2
{
  u32 a;
  u32 b;
};

int infoleak_stack_missing_a_field(void *dst, u32 v)
{
  struct infoleak_2 st;


  st.a = v;

  if (copy_to_user(dst, &st, sizeof(st)))

    return -14;
  return 0;
}

int infoleak_heap_missing_a_field(void *dst, u32 v)
{
  struct infoleak_2 *heapbuf = kmalloc(sizeof(*heapbuf), 0);
  heapbuf->a = v;

  if (copy_to_user(dst, heapbuf, sizeof(*heapbuf)))

    {
      kfree(heapbuf);
      return -14;
    }
  kfree(heapbuf);
  return 0;
}

struct infoleak_3
{
  u8 a;

  u32 b;
};

int infoleak_stack_padding(void *dst, u8 p, u32 q)
{
  struct infoleak_3 st;


  st.a = p;
  st.b = q;

  if (copy_to_user(dst, &st, sizeof(st)))

    return -14;
  return 0;
}

int infoleak_stack_unchecked_err(void *dst, void *src)
{
  struct infoleak_buf st;






  int err = copy_from_user (&st, src, sizeof(st));
  err |= copy_to_user (dst, &st, sizeof(st));



  if (err)
    return -14;
  return 0;
}

struct infoleak_4
{
  union {
    u8 f1;
    u32 f2;
  } u;
};

int infoleak_stack_union(void *dst, u8 v)
{
  struct infoleak_4 st;




  st.u.f1 = v;
  if (copy_to_user(dst, &st, sizeof(st)))

    return -14;
  return 0;
}

struct infoleak_5
{
  void *ptr;
};

int infoleak_stack_kernel_ptr(void *dst, void *kp)
{
  struct infoleak_5 st;

  st.ptr = kp;
  if (copy_to_user(dst, &st, sizeof(st)))
    return -14;
  return 0;
}
