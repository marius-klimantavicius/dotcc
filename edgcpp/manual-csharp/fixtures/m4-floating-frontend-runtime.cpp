// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
double infinity = 1.0 / 0.0;
float tiny = 0x1p-149f / 2.0f;
float overflow = 0x1p127f * 2.0f;
double divide(double value) { return value / 0.0; }
static_assert(sizeof(1.0 / 0.0) == 8);
