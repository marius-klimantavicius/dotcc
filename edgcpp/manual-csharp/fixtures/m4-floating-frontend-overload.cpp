// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
constexpr int pick(double) { return 8; }
constexpr int pick(int) { return 4; }
static_assert(pick(1.0f) == 8);
constexpr int mixed() { int value = 3; value *= 1.5f; return value; }
static_assert(mixed() == 4);
