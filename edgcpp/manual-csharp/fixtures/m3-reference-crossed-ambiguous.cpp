// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored crossed cv bindings: neither candidate wins on every argument.
int choose(int&, const int&);
long choose(const int&, int&);
int use(int value) { return choose(value, value); }
