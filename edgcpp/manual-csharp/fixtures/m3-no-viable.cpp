// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// No source-visible overload has the required arity.
int pick(int);
int pick(int, int);
int entry() { return pick(); }
