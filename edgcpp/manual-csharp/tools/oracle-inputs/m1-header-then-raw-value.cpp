#define HI __has_include
#if HI(<outer/*unterminated.h>)
constexpr char value[] = R"(one\
two)";
static_assert(sizeof(value) == 9);
static_assert(value[3] == '\\');
static_assert(value[4] == '\n');
#endif
