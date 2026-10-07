#include <stdlib.h>
#include <string.h>

char *strdup(const char *s)
{
	size_t l = strlen(s);
	char *d = (char *)malloc(l+1);
	if (!d) return NULL;
	return (char *)memcpy(d, s, l+1);
}
