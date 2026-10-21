// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
constexpr int read(bool run){int value; if(run) value+=1; else value=3; return value;}
static_assert(read(true)==1);
