// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
constexpr int f(){char cells[]="ab";cells[3]='Z';return 1;}
static_assert(f()==1);
