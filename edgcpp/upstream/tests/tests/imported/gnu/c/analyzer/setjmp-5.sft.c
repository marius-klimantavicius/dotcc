//type: fp
//options: 
# 0 "./analyzer/setjmp-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/setjmp-5.c"




# 1 "./analyzer/test-setjmp.h" 1
# 13 "./analyzer/test-setjmp.h"
       
# 14 "./analyzer/test-setjmp.h" 3


# 15 "./analyzer/test-setjmp.h" 3
struct __jmp_buf_tag {
  char buf[1];
};
typedef struct __jmp_buf_tag jmp_buf[1];
typedef struct __jmp_buf_tag sigjmp_buf[1];

extern int setjmp(jmp_buf env);
extern int sigsetjmp(sigjmp_buf env, int savesigs);

extern void longjmp(jmp_buf env, int val);
extern void siglongjmp(sigjmp_buf env, int val);
# 6 "./analyzer/setjmp-5.c" 2
# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/stddef.h" 3 4
} max_align_t;
# 7 "./analyzer/setjmp-5.c" 2
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
# 8 "./analyzer/setjmp-5.c" 2

static jmp_buf env;

static void inner (void)
{
  
# 13 "./analyzer/setjmp-5.c" 3
 setjmp(
# 13 "./analyzer/setjmp-5.c"
 env
# 13 "./analyzer/setjmp-5.c" 3
 )
# 13 "./analyzer/setjmp-5.c"
             ;
}

void outer (void)
{
  int i;

  inner ();

  longjmp (env, 42);
}
