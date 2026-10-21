// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
int unknown(int& value){static_assert((value=2)==2); return value;}
