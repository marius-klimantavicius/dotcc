// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// The nearest nonfunction binding hides the whole outer overload binding.
int pick(int);
int pick(long);
int entry(int pick) { return pick(1); }
