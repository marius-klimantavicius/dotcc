#ifndef VARIADIC_CALLBACK_API_H
#define VARIADIC_CALLBACK_API_H
#include <stdarg.h>
typedef int (*Callback)(int, ...);
typedef int (*Unary)(int);
int consume(int mode, ...);
int read_cursor(va_list ap);
#endif
