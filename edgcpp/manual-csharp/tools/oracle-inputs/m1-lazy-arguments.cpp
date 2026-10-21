#define BAD(x,y) x
#define IGNORE(...) 7
#define STR(...) #__VA_ARGS__
#define S(...) #__VA_OPT__(a)
unused IGNORE(BAD(1))
stringized STR(BAD(1))
vaopt S() S(x)
