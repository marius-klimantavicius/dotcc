#include "algorithms_compat.h"
#include <stdlib.h>

long long llabs(long long a)
{
	return a>0 ? a : -a;
}
