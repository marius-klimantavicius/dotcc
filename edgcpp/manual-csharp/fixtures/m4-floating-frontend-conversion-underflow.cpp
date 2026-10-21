// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
constexpr float narrow(double value) { return value; }
constexpr float lost = narrow(0x1p-150);
