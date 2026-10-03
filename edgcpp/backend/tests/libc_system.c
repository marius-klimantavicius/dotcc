#include <stdlib.h>
#include <stdio.h>
#include <errno.h>
#ifdef __cplusplus
extern "C" {
#endif
int system_call(const char *command)
{
 int (*address)(const char *)=system;
 return address(command);
}
int system_environment(const char *name,const char *value){return setenv(name,value,1);}
int system_error(void){return errno;}
int system_stdout(const char *path){return freopen(path,"w",stdout)==stdout?0:-1;}
#ifdef __cplusplus
}
#endif
