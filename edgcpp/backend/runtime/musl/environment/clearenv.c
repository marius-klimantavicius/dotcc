#include "environment_compat.h"
#define _GNU_SOURCE
#include <stdlib.h>


int clearenv()
{
	char **e = __environ;
	__environ = 0;
	if (e) while (*e) __env_rm_add(*e++, 0);
	return 0;
}
