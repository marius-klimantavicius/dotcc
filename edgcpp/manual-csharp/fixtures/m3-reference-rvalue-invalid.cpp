// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored invalid related-type rvalue reference binding to an lvalue.
int object = 3;
int&& invalid = object;
