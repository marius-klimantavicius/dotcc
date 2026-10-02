#include "algorithms_compat.h"
#include <inttypes.h>

intmax_t imaxabs(intmax_t a)
{
	return a>0 ? a : -a;
}
