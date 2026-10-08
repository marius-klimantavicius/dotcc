#include "macros.h"

int before_redefinition(void) { return API_ALIAS; }

#undef API_VALUE
#define API_VALUE 23
#undef API_NEVER_DEFINED

int after_redefinition(void) { return API_ALIAS; }

int main(void)
{
    int API_CAT(lo, cal) = 7;
    if (before_redefinition() != 17 || after_redefinition() != 23) return 1;
    if (API_ADD(2, 3) != 5 || API_VARIADIC(2, 3) != 5) return 2;
    if (API_MULTI(local) != 11 || API_COMMENT != 5 || API_SAME != 9) return 3;
    API_EMPTY
    return 0;
}
