//type: fp
//options: 
# 0 "./analyzer/error-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/error-1.c"
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
# 2 "./analyzer/error-1.c" 2

extern int errno;

extern void error (int __status, int __errnum, const char *__format, ...)
     __attribute__ ((__format__ (__printf__, 3, 4)));

extern void error_at_line (int __status, int __errnum, const char *__fname,
      unsigned int __lineno, const char *__format, ...)
     __attribute__ ((__format__ (__printf__, 5, 6)));



void test_1 (int st)
{
  error (st, errno, "test");
  __analyzer_eval (st == 0);
}



void test_2 (int st)
{
  error (0, errno, "test");
  __analyzer_dump_path ();
}



void test_3 (int st)
{
  error (1, errno, "test");
  __analyzer_dump_path ();
}



void test_4 (int st)
{
  if (st)
    {
      error (st, errno, "nonzero branch");
      __analyzer_dump_path ();
    }
  else
    {
      error (st, errno, "zero branch");
      __analyzer_dump_path ();
    }
}



void test_5 (int st)
{
  error_at_line (st, errno, "./analyzer/error-1.c", 56, "test");
  __analyzer_eval (st == 0);
}



void test_6 (int st, const char *str)
{
  error (st, errno, "test: %s", str);
  __analyzer_eval (st == 0);
}

char *test_error_unterminated (int st)
{
  char fmt[3] = "abc";
  error (st, errno, fmt);

}

char *test_error_at_line_unterminated (int st, int errno)
{
  char fmt[3] = "abc";
  error_at_line (st, errno, "./analyzer/error-1.c", 78, fmt);

}

char *test_error_uninitialized (int st, int errno)
{
  char fmt[16];
  error (st, errno, fmt);

}

char *test_error_at_line_uninitialized (int st, int errno)
{
  char fmt[16];
  error_at_line (st, errno, "./analyzer/error-1.c", 92, fmt);

}
