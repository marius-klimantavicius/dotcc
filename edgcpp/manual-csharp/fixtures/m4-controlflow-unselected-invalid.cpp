// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
constexpr int invalid(){if(false){const int value=1; value=2;} return 1;}
static_assert(invalid()==1);
