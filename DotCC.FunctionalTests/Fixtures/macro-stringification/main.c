#include <stdio.h>
#define STR(x) #x
#define XSTR(x) STR(x)
#define VALUE 42
#define JOIN(x) a+x
#define SPACED(x) a + x
#define OP ->>
#define VARSTR(...) #__VA_ARGS__
int main(void) {
    puts(STR(->));
    puts(STR(->>));
    puts(STR(-> >));
    puts(STR(a+b));
    puts(STR( a  +  b ));
    puts(STR(a/**/b));
    puts(STR(VALUE));
    puts(XSTR(VALUE));
    puts(XSTR(JOIN(b)));
    puts(XSTR(SPACED(b)));
    puts(XSTR(OP));
    puts(VARSTR(a,b , c));
    puts(STR("a\\b\"c"));
    return 0;
}
