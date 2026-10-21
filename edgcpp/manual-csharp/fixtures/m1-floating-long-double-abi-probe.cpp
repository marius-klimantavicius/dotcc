// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Native target witness only: this module DOES NOT implement long double.
// Proposed Linux-x86_64 profile: extended80 precision64 stored in sixteen bytes.
static_assert(sizeof(long double) == 16);
static_assert(0x1p63L + 1.0L > 0x1p63L);
static_assert(0x1p64L + 1.0L == 0x1p64L);
static_assert(0x1p16383L / 0x1p16382L == 2.0L);
static_assert(0x1p-16445L > 0.0L);
