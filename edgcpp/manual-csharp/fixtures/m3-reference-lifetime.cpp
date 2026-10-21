// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored lifetime hazards: valid binding with warnings, not language errors.
int& local_reference() { int object = 3; return object; }
int& parameter_reference(int object) { return object; }
const int& temporary_reference() { return 5; }
const int& named_temporary_reference() {
    const int& first = 7;
    const int& second = first;
    return second;
}
int& named_rvalue_reference() { int&& value = 9; return value; }
const int& static_temporary = 11;
const int& static_temporary_reference() { return static_temporary; }
