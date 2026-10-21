// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Valid ordinary runtime syntax; required constant evaluation exceeds cost.
constexpr int forever(bool run){while(run){} return 1;}
static_assert(forever(true)==1);
