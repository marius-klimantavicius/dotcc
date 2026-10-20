// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored manual frontend acceptance input; header-free C++20 / LP64.
using Count = long;
Count widen(int value);
Count widen(const int value) {
    Count result = value;
    {
        int result = 7;
        result + 1;
    }
    return result;
}
Count entry() {
    int value = 3;
    return widen(value);
}
