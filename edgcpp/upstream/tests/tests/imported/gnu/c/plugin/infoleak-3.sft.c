//type: fp
//options: 
# 0 "./plugin/infoleak-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./plugin/infoleak-3.c"
# 9 "./plugin/infoleak-3.c"
# 1 "./plugin/../analyzer/analyzer-decls.h" 1
# 20 "./plugin/../analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./plugin/../analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 10 "./plugin/infoleak-3.c" 2

typedef long unsigned int size_t;

# 1 "./plugin/test-uaccess.h" 1
# 9 "./plugin/test-uaccess.h"
extern long copy_from_user(void *to, const void *from, long n);
extern long copy_to_user(void *to, const void *from, long n);
# 14 "./plugin/infoleak-3.c" 2

typedef unsigned int u32;
# 24 "./plugin/infoleak-3.c"
struct st
{
  u32 a;
  u32 b;
};



void test_1_full_init (void *dst, u32 x, u32 y, unsigned long in_sz)
{
  struct st s;
  s.a = x;
  s.b = y;
  unsigned long copy_sz = ({ unsigned long __min1 = (in_sz); unsigned long __min2 = (sizeof(s)); __min1 < __min2 ? __min1: __min2; });
  copy_to_user(dst, &s, copy_sz);
}

void test_1_partial_init (void *dst, u32 x, u32 y, unsigned long in_sz)
{
  struct st s;
  s.a = x;

  unsigned long copy_sz = ({ unsigned long __min1 = (in_sz); unsigned long __min2 = (sizeof(s)); __min1 < __min2 ? __min1: __min2; });
  copy_to_user(dst, &s, copy_sz);
}



void test_2_full_init (void *dst, u32 x, u32 y, unsigned long in_sz)
{
  struct st s;
  s.a = x;
  s.b = y;
  unsigned long copy_sz = ({ unsigned long __min1 = (sizeof(s)); unsigned long __min2 = (in_sz); __min1 < __min2 ? __min1: __min2; });
  copy_to_user(dst, &s, copy_sz);
}

void test_2_partial_init (void *dst, u32 x, u32 y, unsigned long in_sz)
{
  struct st s;
  s.a = x;

  unsigned long copy_sz = ({ unsigned long __min1 = (sizeof(s)); unsigned long __min2 = (in_sz); __min1 < __min2 ? __min1: __min2; });
  copy_to_user(dst, &s, copy_sz);
}



void test_3_full_init (void *dst, u32 x, u32 y, int in_sz)
{
  struct st s;
  s.a = x;
  s.b = y;
  int copy_sz = ({ unsigned int __min1 = (in_sz); unsigned int __min2 = (sizeof(s)); __min1 < __min2 ? __min1: __min2; });
  copy_to_user(dst, &s, copy_sz);
}

void test_3_partial_init (void *dst, u32 x, u32 y, int in_sz)
{
  struct st s;
  s.a = x;

  int copy_sz = ({ unsigned int __min1 = (in_sz); unsigned int __min2 = (sizeof(s)); __min1 < __min2 ? __min1: __min2; });
  copy_to_user(dst, &s, copy_sz);
}



void test_4_full_init (void *dst, u32 x, u32 y, size_t in_sz)
{
  struct st s;
  s.a = x;
  s.b = y;

  size_t copy_sz = in_sz;
  if (copy_sz > sizeof(s))
    copy_sz = sizeof(s);

  copy_to_user(dst, &s, copy_sz);
}

void test_4_partial_init (void *dst, u32 x, u32 y, size_t in_sz)
{
  struct st s;
  s.a = x;


  size_t copy_sz = in_sz;
  if (copy_sz > sizeof(s))
    copy_sz = sizeof(s);

  copy_to_user(dst, &s, copy_sz);
}



void test_5_full_init (void *dst, u32 x, u32 y, int in_sz)
{
  struct st s;
  s.a = x;
  s.b = y;

  int copy_sz = in_sz;
  if (copy_sz > sizeof(s))
    copy_sz = sizeof(s);
  copy_to_user(dst, &s, copy_sz);
}



void test_5_partial_init (void *dst, u32 x, u32 y, int in_sz)
{
  struct st s;
  s.a = x;


  int copy_sz = in_sz;
  if (copy_sz > sizeof(s))
    copy_sz = sizeof(s);

  copy_to_user(dst, &s, copy_sz);
}
