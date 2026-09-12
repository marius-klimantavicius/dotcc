#ifndef _STRING_H
#define _STRING_H

/* dotcc's <string.h> — string and memory operations. Implementations
   live in DotCC.Libc/Libc.cs (strlen/strcmp/strcpy + the mem* trio) and
   DotCC.Libc/StringLib.cs (everything else), spliced into every emitted
   program via the embedded-resource runtime block (see CLAUDE.md for the
   architecture). The signatures here declare the surface so the parser
   accepts `#include <string.h>` and knows the prototypes. */

#include <stddef.h>  /* size_t */

/* Length / comparison / copy. */
int strlen(const char* s);
size_t strnlen(const char* s, size_t maximum);
int strcmp(const char* a, const char* b);
int strncmp(const char* a, const char* b, size_t n);
int strcoll(const char* a, const char* b);
/* POSIX (home: <strings.h>; glibc exposes them here too, which is what
   portable code relies on — chibi calls them with only <string.h>). */
int strcasecmp(const char* a, const char* b);
int strncasecmp(const char* a, const char* b, size_t n);
char* strcpy(char* dst, const char* src);
char* strncpy(char* dst, const char* src, size_t n);

/* Concatenation. */
char* strcat(char* dst, const char* src);
char* strncat(char* dst, const char* src, size_t n);

/* Search. */
char* strchr(const char* s, int c);
char* strrchr(const char* s, int c);
char* strstr(const char* haystack, const char* needle);
int strspn(const char* s, const char* accept);
int strcspn(const char* s, const char* reject);
char* strpbrk(const char* s, const char* accept);

/* Tokenize — reentrant primitive (strtok_r) + stateful wrapper (strtok).
   Prefer strtok_r: it takes an explicit save slot, so it is thread-safe
   and re-entrant. */
char* strtok_r(char* str, const char* delim, char** saveptr);
char* strtok(char* str, const char* delim);

/* Error-number -> message text (see <errno.h>). */
char* strerror(int errnum);

/* Memory. */
void* memset(void* dst, int value, size_t count);
void* memcpy(void* dst, const void* src, size_t count);
void* memmove(void* dst, const void* src, size_t count);
int memcmp(const void* a, const void* b, size_t count);
void* memchr(const void* s, int c, size_t count);

#endif
