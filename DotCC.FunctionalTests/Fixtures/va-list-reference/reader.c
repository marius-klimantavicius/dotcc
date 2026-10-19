#include "cursor.h"
int next(va_list *cursor) { return va_arg(*cursor,int); }
int relay(va_list *cursor) { return next(cursor); }
int optional(va_list *cursor) { if (cursor != 0) return next(cursor); return 11; }
