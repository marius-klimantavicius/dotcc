//type: fp
//options: 
# 0 "./analyzer/null-terminated-strings-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/null-terminated-strings-1.c"
# 1 "./analyzer/analyzer-decls.h" 1
# 20 "./analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 2 "./analyzer/null-terminated-strings-1.c" 2


typedef long unsigned int size_t;

void test_terminated (void)
{
  __analyzer_eval (__analyzer_get_strlen ("abc") == 3);
}

void test_unterminated (void)
{
  char buf[3] = "abc";
  __analyzer_get_strlen (buf);


}

void test_embedded_nuls (void)
{

  char buf[9] = "abc\0pq\0xy";
  __analyzer_eval (__analyzer_get_strlen (buf) == 3);
  __analyzer_eval (__analyzer_get_strlen (buf + 1) == 2);
  __analyzer_eval (__analyzer_get_strlen (buf + 2) == 1);
  __analyzer_eval (__analyzer_get_strlen (buf + 3) == 0);
  __analyzer_eval (__analyzer_get_strlen (buf + 4) == 2);
  __analyzer_eval (__analyzer_get_strlen (buf + 5) == 1);
  __analyzer_eval (__analyzer_get_strlen (buf + 6) == 0);
  __analyzer_get_strlen (buf + 7);


}

void test_before_start_of_buffer (void)
{
  const char *buf = "abc";
  __analyzer_get_strlen (buf - 1);


}

void test_after_end_of_buffer (void)
{
  const char *buf = "abc";
  __analyzer_get_strlen (buf + 4);


}

void test_fully_initialized_but_unterminated (void)
{
  char buf[3];
  buf[0] = 'a';
  buf[1] = 'b';
  buf[2] = 'c';
  __analyzer_get_strlen (buf);

}

void test_uninitialized (void)
{
  char buf[16];
  __analyzer_get_strlen (buf);

}

void test_partially_initialized (void)
{
  char buf[16];
  buf[0] = 'a';
  __analyzer_get_strlen (buf);

}

char *test_dynamic_1 (void)
{
  const char *kvstr = "NAME=value";
  size_t len = __builtin_strlen (kvstr);
  char *ptr = __builtin_malloc (len + 1);
  if (!ptr)
    return ((void *)0);
  __builtin_memcpy (ptr, kvstr, len);
  ptr[len] = '\0';
  __analyzer_eval (__analyzer_get_strlen (ptr) == 10);

  return ptr;
}

char *test_dynamic_2 (void)
{
  const char *kvstr = "NAME=value";
  size_t len = __builtin_strlen (kvstr);
  char *ptr = __builtin_malloc (len + 1);
  if (!ptr)
    return ((void *)0);
  __builtin_memcpy (ptr, kvstr, len);

  __analyzer_get_strlen (ptr);

  return ptr;
}

char *test_dynamic_3 (const char *src)
{
  size_t len = __builtin_strlen (src);
  char *ptr = __builtin_malloc (len + 1);
  if (!ptr)
    return ((void *)0);
  __builtin_memcpy (ptr, src, len);
  ptr[len] = '\0';
  __analyzer_eval (__analyzer_get_strlen (ptr) == len);

  return ptr;
}

char *test_dynamic_4 (const char *src)
{
  size_t len = __builtin_strlen (src);
  char *ptr = __builtin_malloc (len + 1);
  if (!ptr)
    return ((void *)0);
  __builtin_memcpy (ptr, src, len);

  __analyzer_get_strlen (ptr);

  return ptr;
}

void test_symbolic_ptr (const char *ptr)
{
  __analyzer_describe (0, __analyzer_get_strlen (ptr));
}

void test_symbolic_offset (size_t idx)
{
  __analyzer_describe (0, __analyzer_get_strlen ("abc" + idx));
}

void test_casts (void)
{
  int i = 42;
  const char *p = (const char *)&i;
  __analyzer_eval (__analyzer_get_strlen (p) == 0);
  __analyzer_eval (__analyzer_get_strlen (p + 1) == 0);
}

void test_filled_nonzero (void)
{
  char buf[10];
  __builtin_memset (buf, 'a', 10);
  __analyzer_get_strlen (buf);
}

void test_filled_zero (void)
{
  char buf[10];
  __builtin_memset (buf, 0, 10);
  __analyzer_eval (__analyzer_get_strlen (buf) == 0);

  __analyzer_eval (__analyzer_get_strlen (buf + 1) == 0);

}

void test_filled_symbolic (int c)
{
  char buf[10];
  __builtin_memset (buf, c, 10);
  __analyzer_eval (__analyzer_get_strlen (buf) == 0);
}
