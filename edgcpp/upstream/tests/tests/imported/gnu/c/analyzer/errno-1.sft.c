//type: fp
//options: 
# 0 "./analyzer/errno-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/errno-1.c"
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

# 2 "./analyzer/errno-1.c" 2
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
# 3 "./analyzer/errno-1.c" 2

extern void external_fn (void);

int test_reading_errno (void)
{
  return 
# 8 "./analyzer/errno-1.c" 3 4
        (*__errno_location ())
# 8 "./analyzer/errno-1.c"
             ;
}

void test_setting_errno (int val)
{
  
# 13 "./analyzer/errno-1.c" 3 4
 (*__errno_location ()) 
# 13 "./analyzer/errno-1.c"
       = val;
}

void test_storing_to_errno (int val)
{
  __analyzer_eval (
# 18 "./analyzer/errno-1.c" 3 4
                  (*__errno_location ()) 
# 18 "./analyzer/errno-1.c"
                        == val);
  
# 19 "./analyzer/errno-1.c" 3 4
 (*__errno_location ()) 
# 19 "./analyzer/errno-1.c"
       = val;
  __analyzer_eval (
# 20 "./analyzer/errno-1.c" 3 4
                  (*__errno_location ()) 
# 20 "./analyzer/errno-1.c"
                        == val);
  external_fn ();
  __analyzer_eval (
# 22 "./analyzer/errno-1.c" 3 4
                  (*__errno_location ()) 
# 22 "./analyzer/errno-1.c"
                        == val);
}
