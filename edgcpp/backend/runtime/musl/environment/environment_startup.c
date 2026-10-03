#include "environment_compat.h"
/* The compiler's once-only startup path initializes owner globals before entry.
   Priority zero is reserved for implementation initialization, before user
   constructor priorities101..65535. Native aliases are not required: internal musl __environ spells this cell. */
char **environ;
__attribute__((constructor(0))) static void __edg_initialize_environment(void)
{ environ=__edg_environment_snapshot(); }
