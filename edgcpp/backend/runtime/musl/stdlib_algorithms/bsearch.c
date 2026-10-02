#include "algorithms_compat.h"
#include <stdlib.h>

void *bsearch(const void *key, const void *base, size_t nel, size_t width, int (*cmp)(const void *, const void *))
{
	void *probe;
	int sign;
	while (nel > 0) {
		probe = (char *)base + width*(nel/2);
		sign = cmp(key, probe);
		if (sign < 0) {
			nel /= 2;
		} else if (sign > 0) {
			base = (char *)probe + width;
			nel -= nel/2+1;
		} else {
			return probe;
		}
	}
	return NULL;
}
