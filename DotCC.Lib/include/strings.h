#ifndef _DOTCC_STRINGS_H
#define _DOTCC_STRINGS_H

/* The supported POSIX case-insensitive operations share declarations and
   C-locale runtime implementations with dotcc's string.h. */
#include <string.h>

void bzero(void *destination, size_t length);

#endif
