//type: fp
//options: 
# 0 "./analyzer/untracked-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/untracked-1.c"


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
# 4 "./analyzer/untracked-1.c" 2

struct st
{
  const char *m_filename;
  int m_line;
};

typedef struct boxed_int { int value; } boxed_int;

extern void extern_fn (struct st *);
static void __attribute__((noinline)) internal_fn (struct st *) {}
extern int extern_get_int (void);
extern void extern_fn_char_ptr (const char *);

void test_0 (void)
{


  static struct st s1 = { "./analyzer/untracked-1.c", 22 };
}

void test_1 (void)
{
  static struct st s1 = { "./analyzer/untracked-1.c", 27 };
  extern_fn (&s1);
}

static struct st s2 = { "./analyzer/untracked-1.c", 31 };

void test_2 (void)
{
  extern_fn (&s2);
}

void test_3 (void)
{
  struct st s3 = { "./analyzer/untracked-1.c", 40 };
  extern_fn (&s3);
}

void test_3a (void)
{
  struct st s3a = { "foo.c", 42 };
  __analyzer_eval (s3a.m_filename[0] == 'f');
  __analyzer_eval (s3a.m_line == 42);
  extern_fn (&s3a);
  __analyzer_eval (s3a.m_filename[0] == 'f');
  __analyzer_eval (s3a.m_line == 42);
}

extern void called_by_test_4 (int *);

int test_4 (void)
{
  int i;
  called_by_test_4 (&i);
  return i;
}

void test_5 (int i)
{
  boxed_int bi5 = { i };
}

int test_6 (int i)
{
  static boxed_int bi6;
  bi6.value = i;
  return bi6.value;
}

int test_7 (void)
{
  boxed_int bi7;
  return bi7.value;
}

void test_8 (void)
{
  static struct st s8 = { "./analyzer/untracked-1.c", 83 };
  extern_fn (&s8);
  extern_fn (&s8);
}

void test_9 (void)
{
  static struct st s9 = { "./analyzer/untracked-1.c", 90 };
  internal_fn (&s9);
}

int test_10 (void)
{
  static struct st s10 = { "./analyzer/untracked-1.c", 96 };
  extern_fn (&s10);
  return s10.m_line;
}

int test_11 (void)
{
  static struct st s10 = { "./analyzer/untracked-1.c", 103 };
  s10.m_line = extern_get_int ();
  return 42;
}

int test_12 (void (*fnptr) (struct st *))
{
  static struct st s12 = { "./analyzer/untracked-1.c", 110 };
  fnptr (&s12);
}

void test_13 (void)
{
  extern_fn_char_ptr (__func__);
}

char t14_global_unused[100];
static char t14_static_unused[100];
char t14_global_used[100];
static char t14_static_used[100];
void test_14 (void)
{
  extern_fn_char_ptr (t14_global_unused);
  extern_fn_char_ptr (t14_static_unused);
  extern_fn_char_ptr (t14_global_used);
  __analyzer_eval (t14_global_used[0] == '\0');
  extern_fn_char_ptr (t14_static_used);
  __analyzer_eval (t14_static_used[0] == '\0');
}
