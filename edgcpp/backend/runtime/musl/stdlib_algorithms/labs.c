#include "algorithms_compat.h"
#include <stdlib.h>

long labs(long a)
{
	return a>0 ? a : -a;
}
