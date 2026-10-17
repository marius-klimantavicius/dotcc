#include "selected-constructor-boundary.hpp"

static int next_serial;

// This out-of-class complete constructor is the actual cross-TU dependency.
// It has no explicit noexcept annotation and no nontrivial member lifetime.
BoundaryCell::BoundaryCell()
{
    canary = -0x6123456789ABCDE0LL;
    serial = ++next_serial;
    kind = 201;
}
