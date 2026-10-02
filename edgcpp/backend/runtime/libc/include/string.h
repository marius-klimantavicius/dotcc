/* C17 string declarations plus existing bounded/POSIX extensions. */
#ifndef __EDG_DOT_LIBC_STRING_H
#define __EDG_DOT_LIBC_STRING_H
#ifdef __cplusplus
extern "C" {
#endif
#include <stddef.h>
void *memcpy(void *, const void *, size_t);
void *memmove(void *, const void *, size_t);
void *memset(void *, int, size_t);
int memcmp(const void *, const void *, size_t);
void *memchr(const void *, int, size_t);
size_t strlen(const char *);
size_t strnlen(const char *, size_t);
int strcmp(const char *, const char *);
int strcoll(const char *, const char *);
size_t strxfrm(char *, const char *, size_t);
char *strerror(int);
int strerror_r(int, char *, size_t);
int strncmp(const char *, const char *, size_t);
char *strcpy(char *, const char *);
char *strncpy(char *, const char *, size_t);
char *strcat(char *, const char *);
char *strncat(char *, const char *, size_t);
char *strchr(const char *, int);
char *strrchr(const char *, int);
char *strstr(const char *, const char *);
char *strpbrk(const char *, const char *);
size_t strspn(const char *, const char *);
size_t strcspn(const char *, const char *);
char *strtok_r(char *, const char *, char **);
char *strtok(char *, const char *);
#ifdef __cplusplus
}
#endif
#endif
