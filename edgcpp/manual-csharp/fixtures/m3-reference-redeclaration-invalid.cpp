// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored conflicting return reference types for the same function signature.
int& incompatible(int&);
const int& incompatible(int&);
