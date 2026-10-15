#ifndef ROSLYN_CSHARP_T2_FIXTURE_VA_H
#define ROSLYN_CSHARP_T2_FIXTURE_VA_H

/* The pinned GNU profile captures these as real selected va_* operations. */
typedef __builtin_va_list fixture_va_list;
#define fixture_va_start(list, last) __builtin_va_start(list, last)
#define fixture_va_arg(list, type) __builtin_va_arg(list, type)
#define fixture_va_copy(destination, source) __builtin_va_copy(destination, source)
#define fixture_va_end(list) __builtin_va_end(list)

#endif
