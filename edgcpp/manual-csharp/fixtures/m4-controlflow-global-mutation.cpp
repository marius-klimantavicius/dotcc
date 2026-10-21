// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
int global=1;
constexpr int write(int& value){value=2; return value;}
static_assert(write(global)==2);
