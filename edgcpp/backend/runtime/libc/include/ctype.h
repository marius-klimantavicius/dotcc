/* C17 declarations; imports remain explicitly implemented and ABI checked. */
#ifndef __EDG_DOT_LIBC_CTYPE_H
#define __EDG_DOT_LIBC_CTYPE_H
#ifdef __cplusplus
extern "C" {
#endif
int isascii(int);
int isalnum(int);
int isalpha(int);
int isblank(int);
int iscntrl(int);
int isdigit(int);
int isgraph(int);
int islower(int);
int isprint(int);
int ispunct(int);
int isspace(int);
int isupper(int);
int isxdigit(int);
int tolower(int);
int toupper(int);
#ifdef __cplusplus
}
#endif
#endif
