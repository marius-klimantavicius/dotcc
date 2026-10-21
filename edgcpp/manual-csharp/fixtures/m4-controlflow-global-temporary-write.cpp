// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
int&& global=1;
constexpr int f(){global=2; return 1;}
static_assert(f()==1);
