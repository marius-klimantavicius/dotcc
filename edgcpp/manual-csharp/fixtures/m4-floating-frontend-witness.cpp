// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
#include "m4-floating-frontend-gate.cpp"
constexpr int target_kind(float) { return 4; }
constexpr int target_kind(double) { return 8; }
static_assert(target_kind(accumulate(1.0f)) == 4);
static_assert(target_kind(accumulate(1.0)) == 8);
static_assert(target_kind(run()) == 4);
