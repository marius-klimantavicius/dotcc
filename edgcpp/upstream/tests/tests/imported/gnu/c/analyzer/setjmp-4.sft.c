//type: fp
//options: 
# 0 "./analyzer/setjmp-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/setjmp-4.c"




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
# 6 "./analyzer/setjmp-4.c" 2
# 1 "./analyzer/analyzer-decls.h" 1
# 20 "./analyzer/analyzer-decls.h"

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
# 7 "./analyzer/setjmp-4.c" 2

extern int foo (int) __attribute__ ((__pure__));
static jmp_buf buf;

void inner (int x)
{
  foo (x);
  longjmp (buf, 1);
  foo (x);
}

void outer (int y)
{
  foo (y);
  inner (y);
  foo (y);
}

int main (void)
{
  if (!
# 27 "./analyzer/setjmp-4.c" 3
      setjmp(
# 27 "./analyzer/setjmp-4.c"
      buf
# 27 "./analyzer/setjmp-4.c" 3
      )
# 27 "./analyzer/setjmp-4.c"
                 )
    outer (42);
  else
    __analyzer_dump_path ();
  return 0;
}
