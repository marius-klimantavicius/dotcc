/* Diagnostic output is owner-aware; abnormal termination uses source abort. */
#include <assert.h>
#include <stdlib.h>
#ifdef __cplusplus
extern "C" {
#endif
void __edg_assert_report(const char *,const char *,int,const char *);
void __assert_fail(const char *expression,const char *file,int line,const char *function)
{
 __edg_assert_report(expression,file,line,function);
 abort();
}
#ifdef __cplusplus
}
#endif
