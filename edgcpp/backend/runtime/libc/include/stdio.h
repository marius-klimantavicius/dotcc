/* Portable FILE ABI, version2 positions retain opaque decoder state. */
#ifndef __EDG_DOT_LIBC_STDIO_H
#define __EDG_DOT_LIBC_STDIO_H
#include <stddef.h>
#include <stdarg.h>
typedef struct __edg_FILE FILE;
#define __EDG_FPOS_ABI_VERSION 2
typedef struct __edg_fpos { long offset; unsigned int state, value; } fpos_t;
#define EOF (-1)
#define SEEK_SET 0
#define SEEK_CUR 1
#define SEEK_END 2
#define _IOFBF 0
#define _IOLBF 1
#define _IONBF 2
#define BUFSIZ 8192
#define FOPEN_MAX 1000
#define FILENAME_MAX 4096
#define L_tmpnam 20
#define TMP_MAX 238328
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
FILE *freopen(const char *, const char *, FILE *);
FILE *tmpfile(void);
char *tmpnam(char *);
void rewind(FILE *);
void perror(const char *);
void setbuf(FILE *, char *);
int scanf(const char *, ...);
int fscanf(FILE *, const char *, ...);
int sscanf(const char *, const char *, ...);
int vscanf(const char *, va_list);
int vfscanf(FILE *, const char *, va_list);
int vsscanf(const char *, const char *, va_list);
int asprintf(char **, const char *, ...);
int vasprintf(char **, const char *, va_list);
int printf(const char *, ...);
int fprintf(FILE *, const char *, ...);
int sprintf(char *, const char *, ...);
int snprintf(char *, size_t, const char *, ...);
int vprintf(const char *, va_list);
int vfprintf(FILE *, const char *, va_list);
int vsprintf(char *, const char *, va_list);
int vsnprintf(char *, size_t, const char *, va_list);
#ifdef __cplusplus
}
#endif
#endif
