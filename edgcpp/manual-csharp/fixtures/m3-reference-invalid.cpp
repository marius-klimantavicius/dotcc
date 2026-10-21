// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored invalid binding: related const cannot be dropped through a temporary.
const int object = 3;
int& invalid = object;
