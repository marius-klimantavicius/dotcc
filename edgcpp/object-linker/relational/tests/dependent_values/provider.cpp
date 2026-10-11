#include "program.h"
int score(Dependent<3> *value, MemberGate<Settings> *member) {
    return value->value + value->extra + member->value + member->extra + adjusted<4>();
}
