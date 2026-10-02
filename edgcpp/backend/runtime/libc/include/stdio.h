/* Portable byte-stream ABI. fpos_t will grow with wide-stream conversion state. */
#ifndef __EDG_DOT_LIBC_STDIO_H
#define __EDG_DOT_LIBC_STDIO_H
#include <stddef.h>
typedef struct __edg_FILE FILE;
typedef long fpos_t;
#define EOF (-1)
#define SEEK_SET 0
#define SEEK_CUR 1
#define SEEK_END 2
#define _IOFBF 0
#define _IOLBF 1
#define _IONBF 2
#define BUFSIZ 8192
#ifdef __cplusplus
extern "C" {
#endif
extern FILE *stdin, *stdout, *stderr;
FILE *fopen(const char *, const char *);
int fclose(FILE *);
size_t fread(void *, size_t, size_t, FILE *);
size_t fwrite(const void *, size_t, size_t, FILE *);
int fseek(FILE *, long, int);
long ftell(FILE *);
int fgetpos(FILE *, fpos_t *);
int fsetpos(FILE *, const fpos_t *);
int feof(FILE *);
int ferror(FILE *);
void clearerr(FILE *);
int fflush(FILE *);
int setvbuf(FILE *, char *, int, size_t);
int fgetc(FILE *);
int getc(FILE *);
int ungetc(int, FILE *);
int fputc(int, FILE *);
int putc(int, FILE *);
int fputs(const char *, FILE *);
int puts(const char *);
char *fgets(char *, int, FILE *);
int getchar(void);
int putchar(int);
int remove(const char *);
int rename(const char *, const char *);
#ifdef __cplusplus
}
#endif
#endif
