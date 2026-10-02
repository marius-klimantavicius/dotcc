/* EDG dot-gen portable LP64 ABI: initial audited libc subset. */
#ifndef __EDG_DOT_LIBC_STDDEF_H
#define __EDG_DOT_LIBC_STDDEF_H
typedef unsigned long size_t;
typedef long ptrdiff_t;
#ifndef __cplusplus
typedef int wchar_t;
#endif
typedef struct { long long __ll; long double __ld; } max_align_t;
#ifndef NULL
#ifdef __cplusplus
#define NULL 0
#else
#define NULL ((void*)0)
#endif
#endif
#define offsetof(type, member) __builtin_offsetof(type, member)
#endif
