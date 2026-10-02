//type: fp
//options: 
# 0 "./analyzer/strlen-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/strlen-1.c"


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
# 4 "./analyzer/strlen-1.c" 2

typedef long unsigned int size_t;

static size_t __attribute__((noinline))
call_strlen_1 (const char *p)
{
  return __builtin_strlen (p);
}

void test_string (void)
{
  __analyzer_eval (call_strlen_1 ("abc") == 3);
}

static size_t __attribute__((noinline))
call_strlen_2 (const char *p)
{
  return __builtin_strlen (p);
}

void test_unterminated (void)
{
  const char buf[3] = "abc";
  __analyzer_eval (call_strlen_2 (buf) == 3);
}

void test_uninitialized (void)
{
  char buf[16];
  __builtin_strlen (buf);
}

void test_partially_initialized (void)
{
  char buf[16];
  buf[0] = 'a';
  __builtin_strlen (buf);
}

extern size_t strlen (const char *str);

size_t
test_passthrough (const char *str)
{
  return strlen (str);
}






static size_t __attribute__((noinline))
call_strlen_3 (const char *p)
{
  return __builtin_strlen (p);
}

void test_array_initialization_from_shorter_literal (void)
{
  const char buf[10] = "abc";
  __analyzer_eval (call_strlen_3 (buf) == 3);
  __analyzer_eval (call_strlen_3 (buf + 5) == 0);
  __analyzer_eval (buf[5] == 0);
}

static size_t __attribute__((noinline))
call_strlen_4 (const char *p)
{
  return __builtin_strlen (p);
}

char test_array_initialization_from_longer_literal (void)
{
  const char buf[3] = "abcdefg";
  __analyzer_eval (call_strlen_4 (buf) == 3);
  return buf[5];
}

static size_t __attribute__((noinline))
call_strlen_5 (const char *p)
{
  return __builtin_strlen (p);
}

static size_t __attribute__((noinline))
call_strlen_5a (const char *p)
{
  return __builtin_strlen (p);
}

char test_array_initialization_implicit_length (void)
{
  const char buf[] = "abc";
  __analyzer_eval (call_strlen_5 (buf) == 3);
  __analyzer_eval (call_strlen_5 (buf + 2) == 1);
  __analyzer_eval (call_strlen_5 (buf + 3) == 0);
  __analyzer_eval (buf[0] == 'a');
  __analyzer_eval (buf[3] == 0);
  __analyzer_eval (call_strlen_5a (buf + 4) == 0);
  return buf[4];
}
