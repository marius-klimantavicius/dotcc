// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored native-only proposal for scalar conversions and finite operations.
static_assert(static_cast<float>(16777217) == 16777216.0f);
static_assert(static_cast<double>(9007199254740993ULL) == 9007199254740992.0);
static_assert(static_cast<signed char>(127.75) == 127);
static_assert(static_cast<signed char>(-128.75) == -128);
static_assert(static_cast<unsigned char>(-0.75) == 0);
static_assert(static_cast<double>(0.1f) == 0x1.99999ap-4);
static_assert(1.0f + 0x1p-24f == 1.0f);
static_assert(0x1.000002p0f + 0x1p-24f == 0x1.000004p0f);
static_assert(0x1.000002p0f - 1.0f == 0x1p-23f);
static_assert(0x1.000002p0f * 0x1.000002p0f == 0x1.000004p0f);
static_assert(1.0f / 3.0f == 0x1.555556p-2f);
static_assert(1.0 / 3.0 == 0x1.5555555555555p-2);
