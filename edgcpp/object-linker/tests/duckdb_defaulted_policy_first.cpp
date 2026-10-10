#include "duckdb_defaulted_policy_common.h"
extern int second();
extern int throwing_second();
int policy_failure = 0;
extern "C" void free(void *);
void operator delete(void *p, unsigned long) noexcept { free(p); }
int lower_other_member_first() {
    PolicyDerived<int> value(1);
    // Its unused default constructor must not force an ill-formed initializer.
    PolicyUnusedDefault<PolicyWithoutDefaultMember> unused_default(3);
    return value.value + unused_default.value;
}
int throwing_first() {
    policy_failure = 1;
    try { PolicyMayThrow<int> value; (void)value; }
    catch (int value) { policy_failure = 0; return value; }
    policy_failure = 0; return -1;
}
int first() {
    // The earlier function already lowered this receiver through another member.
    (void)lower_other_member_first();
    PolicyDerived<int> value;
    PolicyMayThrow<int> may_throw;
    PolicyExplicit<int> explicit_policy;
    return value.value + may_throw.value + explicit_policy.value;
}
int main() { return first() + second() != 46 || throwing_first() != 42 || throwing_second() != 42; }
