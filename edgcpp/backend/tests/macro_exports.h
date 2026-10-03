#ifndef DOTGEN_MACRO_EXPORTS_TEST_H
#define DOTGEN_MACRO_EXPORTS_TEST_H
#define API_INT (1 << 12)
#define API_U64 0xffffffffffffffffULL
#define API_NEGATIVE (-17)
#define API_TEXT "macro \xc3\xa9\0tail"
#define API_ALIAS API_INT
static int macro_live_value = 21;
#define API_LIVE macro_live_value
#define API_REDEFINED 1
#undef API_REDEFINED
#define API_REDEFINED 9
#define API_REMOVED 1
#undef API_REMOVED
#define API_EMPTY
#define API_TWICE(value) ((value) + (value))
#define API_AT(pointer, index) ((pointer)[index])
typedef struct MacroPoint { int x; int y; } MacroPoint;
#define API_POINT(xval, yval) make_point((xval), (yval))
static MacroPoint make_point(int x, int y) { MacroPoint point = { x, y }; return point; }
#endif
