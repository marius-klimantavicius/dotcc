// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
constexpr double divide(double value) { return value / 0.0; }
static_assert(divide(1.0) > 0.0);
