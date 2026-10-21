// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored ambiguity: by-value identity has no automatic preference over a reference.
int choose(int);
long choose(const int&);
int use() { return choose(1); }
