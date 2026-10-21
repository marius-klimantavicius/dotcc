// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
int fallback=1;
constexpr int& expired(int& safe,bool leave){if(leave){int value=1; return value;} return safe;}
static_assert((expired(fallback,true)=2)==2);
