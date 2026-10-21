// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
constexpr float halve(float value) { return value / 2.0f; }
constexpr float lost = halve(0x1p-149f);
