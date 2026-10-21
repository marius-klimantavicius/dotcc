// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
constexpr int f(){int&& local=1; local=2; return local;}
static_assert(f()==2);
