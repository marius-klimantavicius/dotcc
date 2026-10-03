#include "environment_compat.h"
#include <stdlib.h>
#include <string.h>
#include <errno.h>

void __env_rm_add(char *old, char *replacement)
{
	static char **env_alloced;
	static size_t env_alloced_n;
	for (size_t i=0; i < env_alloced_n; i++)
		if (env_alloced[i] == old) {
			env_alloced[i] = replacement;
			free(old);
			return;
		} else if (!env_alloced[i] && replacement) {
			env_alloced[i] = replacement;
			replacement = 0;
		}
	if (!replacement) return;
	char **t = (char **)realloc(env_alloced, sizeof *t * (env_alloced_n+1));
	if (!t) return;
	(env_alloced = t)[env_alloced_n++] = replacement;
}

int setenv(const char *var, const char *value, int overwrite)
{
	char *s;
	size_t l1, l2;

	if (!var || !(l1 = __strchrnul(var, '=') - var) || var[l1]) {
		errno = EINVAL;
		return -1;
	}
	if (!overwrite && getenv(var)) return 0;

	l2 = strlen(value);
	s = (char *)malloc(l1+l2+2);
	if (!s) return -1;
	memcpy(s, var, l1);
	s[l1] = '=';
	memcpy(s+l1+1, value, l2+1);
	return __putenv(s, l1, s);
}
