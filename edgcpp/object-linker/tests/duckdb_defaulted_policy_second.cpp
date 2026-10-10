#include "duckdb_defaulted_policy_common.h"
static_assert(noexcept(PolicyDerived<int>()));
static_assert(!noexcept(PolicyMayThrow<int>()));
static_assert(!noexcept(PolicyExplicit<int>()));
int throwing_second() {
    policy_failure = 1;
    try { PolicyMayThrow<int> value; (void)value; }
    catch (int value) { policy_failure = 0; return value; }
    policy_failure = 0; return -1;
}
int second() {
    PolicyDerived<int> value;
    PolicyMayThrow<int> may_throw;
    PolicyExplicit<int> explicit_policy;
    return value.value + may_throw.value + explicit_policy.value;
}
