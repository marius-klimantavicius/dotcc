// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Valid C++, explicitly unsupported by this bounded reference implementation.
template<class T> T& reference_identity(T& value) { return value; }
int object = 3;
int& alias = reference_identity(object);
