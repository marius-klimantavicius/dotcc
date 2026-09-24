#ifndef _DOTCC_LIBGEN_H
#define _DOTCC_LIBGEN_H

/* POSIX path operations may modify their argument and may return static
   storage. Declarations do not select an application-owned native backend. */
char *basename(char *path);
char *dirname(char *path);

#endif
