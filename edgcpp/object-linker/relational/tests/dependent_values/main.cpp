#include "program.h"
int main() {
    Dependent<3> value;
    value.value = 4;
    value.extra = 1;
    MemberGate<Settings> member;
    member.value = 2;
    member.extra = 3;
    if (score(&value, &member) != 15) return 1;
    if (adjusted<8>() != 9) return 2;
    if (adjusted_float<0.25>() != 0.75) return 3;
    return 0;
}
