// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Equal standard integer conversions do not prefer destination width.
int pick(long);
int pick(long long);
int entry() { return pick(1); }
