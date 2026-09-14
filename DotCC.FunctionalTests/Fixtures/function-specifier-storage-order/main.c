#include <stdio.h>
#include <setjmp.h>
static jmp_buf env;
_Noreturn static void jump_back(int value);
inline static int add(int a, int b) { return a + b; }
_Noreturn static void jump_back(int value) { longjmp(env, value); }
int main(void) {
 int result = setjmp(env);
 if (!result) jump_back(add(17, 25));
 printf("returned=%d\n", result);
 return result != 42;
}
