//type: fp
//options: 
# 0 "./analyzer/setjmp-pr93378.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/setjmp-pr93378.c"



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
# 5 "./analyzer/setjmp-pr93378.c" 2


# 6 "./analyzer/setjmp-pr93378.c"
jmp_buf buf;

int
test (void)
{
  if (setjmp (buf) != 0)
    return 0;

  longjmp (buf, 1);
  return 1;
}
