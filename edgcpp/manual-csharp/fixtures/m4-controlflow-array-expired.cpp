// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
constexpr char* dead(){char cells[]="ab";return cells;}
constexpr int f(){dead()[0]='Z';return 1;}
static_assert(f()==1);
