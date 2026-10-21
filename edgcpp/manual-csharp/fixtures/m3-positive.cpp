// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored scalar-overload witness input; no fixture-specific implementation.
int pick(int);
long pick(long);
int pick(const int value) { return value; }
long pick(long value) { return value; }
int exact(int value) { return pick(value); }
int promotion(short value) { return pick(value); }
