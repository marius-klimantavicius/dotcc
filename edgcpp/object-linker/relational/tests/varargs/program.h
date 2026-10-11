typedef __builtin_va_list va_list;
#define va_start(ap, last) __builtin_va_start(ap, last)
#define va_arg(ap, type) __builtin_va_arg(ap, type)
#define va_copy(to, from) __builtin_va_copy(to, from)
#define va_end(ap) __builtin_va_end(ap)
int scan(int count, ...);
int empty(int value, ...);
int raises(int *cleanup, int marker, ...);
typedef int (*Scanner)(int, ...);
Scanner select_scan();
