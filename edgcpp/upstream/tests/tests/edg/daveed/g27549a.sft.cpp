//remark:va_copy in Microsoft mode
//options:--microsoft_v=1940;fp

#include <stdarg.h>

void foo(va_list va) {
    va_list va2;
    va_copy(va2, va);
}

