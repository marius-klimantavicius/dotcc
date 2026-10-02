//type: fp
//options: 
# 0 "./analyzer/isatty-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/isatty-1.c"


# 1 "/usr/include/errno.h" 1 3 4
# 28 "/usr/include/errno.h" 3 4
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
# 29 "/usr/include/errno.h" 2 3 4






# 1 "/usr/include/bits/errno.h" 1 3 4
# 24 "/usr/include/bits/errno.h" 3 4
# 1 "/usr/include/linux/errno.h" 1 3 4
# 1 "/usr/include/asm/errno.h" 1 3 4
# 1 "/usr/include/asm-generic/errno.h" 1 3 4



# 1 "/usr/include/asm-generic/errno-base.h" 1 3 4
# 5 "/usr/include/asm-generic/errno.h" 2 3 4
# 2 "/usr/include/asm/errno.h" 2 3 4
# 2 "/usr/include/linux/errno.h" 2 3 4
# 25 "/usr/include/bits/errno.h" 2 3 4
# 50 "/usr/include/bits/errno.h" 3 4

# 50 "/usr/include/bits/errno.h" 3 4
extern int *__errno_location (void) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__));
# 36 "/usr/include/errno.h" 2 3 4
# 58 "/usr/include/errno.h" 3 4

# 4 "./analyzer/isatty-1.c" 2
# 1 "./analyzer/analyzer-decls.h" 1








# 8 "./analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 32 "./analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);
# 5 "./analyzer/isatty-1.c" 2

extern int isatty(int fd);
extern int close(int fd);

int test_pass_through (int fd)
{
  return isatty (fd);
}

void test_merging (int fd)
{
  isatty (fd);
  __analyzer_dump_exploded_nodes (0);
}

int test_outcomes (int fd)
{
  
# 22 "./analyzer/isatty-1.c" 3 4
 (*__errno_location ()) 
# 22 "./analyzer/isatty-1.c"
       = 0;
  int result = isatty (fd);
  switch (result)
    {
    default:
      __analyzer_dump_path ();
      break;
    case 0:
      __analyzer_dump_path ();
      __analyzer_eval (
# 31 "./analyzer/isatty-1.c" 3 4
                      (*__errno_location ()) 
# 31 "./analyzer/isatty-1.c"
                            > 0);
      break;
    case 1:
      __analyzer_dump_path ();
      __analyzer_eval (
# 35 "./analyzer/isatty-1.c" 3 4
                      (*__errno_location ()) 
# 35 "./analyzer/isatty-1.c"
                            == 0);
      break;
    }
  return result;
}

int test_isatty_on_invalid_fd (void)
{
  
# 43 "./analyzer/isatty-1.c" 3 4
 (*__errno_location ()) 
# 43 "./analyzer/isatty-1.c"
       = 0;
  int result = isatty (-1);
  __analyzer_eval (result == 0);
  __analyzer_eval (
# 46 "./analyzer/isatty-1.c" 3 4
                  (*__errno_location ()) 
# 46 "./analyzer/isatty-1.c"
                        > 0);
  return result;
}

int test_isatty_on_closed_fd (int fd)
{
  close (fd);
  
# 53 "./analyzer/isatty-1.c" 3 4
 (*__errno_location ()) 
# 53 "./analyzer/isatty-1.c"
       = 0;
  int result = isatty (fd);
  __analyzer_eval (result == 0);
  __analyzer_eval (
# 56 "./analyzer/isatty-1.c" 3 4
                  (*__errno_location ()) 
# 56 "./analyzer/isatty-1.c"
                        > 0);
  return result;
}
