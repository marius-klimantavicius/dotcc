// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Each candidate is better on a different argument; summed ranks cannot select.
int pick(int, long, long);
int pick(long, int, int);
int entry() { return pick(1, 1, 1); }
