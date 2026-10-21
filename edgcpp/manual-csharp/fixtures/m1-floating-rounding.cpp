// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored native-only nearest/tie/even/carry witnesses; proposal, unexecuted here.
static_assert(1.000000059604644775390624f == 1.0f);
static_assert(1.000000059604644775390625f == 1.0f);
static_assert(1.000000059604644775390626f == 0x1.000002p0f);
static_assert(1.000000178813934326171875f == 0x1.000004p0f);
static_assert(0x1.ffffffp0f == 2.0f);
static_assert(0x1.00000000000008p0 == 1.0);
static_assert(0x1.00000000000018p0 == 0x1.0000000000002p0);
static_assert(0x1.fffffffffffff8p0 == 2.0);
static_assert(0.1f == 0x1.99999ap-4f && 0.1 == 0x1.999999999999ap-4);
static_assert(1'2.5'0e+0'1f == 125.0f);
static_assert(0xA'B.C'Dp-0'4 == 0xA.BCDp0);
