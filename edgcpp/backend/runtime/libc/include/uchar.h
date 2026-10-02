/* C11 Unicode conversion ABI. Functions require the selected multibyte provider. */
#ifndef __EDG_DOT_LIBC_UCHAR_H
#define __EDG_DOT_LIBC_UCHAR_H
#include <stddef.h>
#include <wchar.h>
#if !defined(__cplusplus) || __cplusplus < 201103L
typedef unsigned short char16_t;
typedef unsigned int char32_t;
#endif
#ifdef __cplusplus
extern "C" {
#endif
size_t c16rtomb(char *, char16_t, mbstate_t *);
size_t mbrtoc16(char16_t *, const char *, size_t, mbstate_t *);
size_t c32rtomb(char *, char32_t, mbstate_t *);
size_t mbrtoc32(char32_t *, const char *, size_t, mbstate_t *);
#ifdef __cplusplus
}
#endif
#endif
