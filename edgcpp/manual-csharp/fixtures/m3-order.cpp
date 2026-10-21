// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored point-of-reference visibility witness.
int pick(long);
int before() { return pick(1); }
int pick(int);
int after() { return pick(1); }
