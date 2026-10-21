// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Direct conversion must retain the tail beyond a double's precision. Native
// host-conversion branch/double-rounding behavior needs its own exact receipt.
constexpr float directly = 1.0000000596046447753906250000000000000001f;
constexpr float via_double = static_cast<float>(1.0000000596046447753906250000000000000001);
static_assert(directly == 0x1.000002p0f);
static_assert(via_double == 1.0f);
