// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored target-correctness witness. The pinned EDG hex carry/reset defect is
// root-authorized as a narrow adaptation: two below-half assertions here are
// known native mismatches, not language-negative/parser-pass evidence. See handoff.
static_assert(0x1p-150f == 0.0f);
static_assert(0x1.000001p-150f == 0x1p-149f);
static_assert(0x3p-150f == 0x1p-148f);
static_assert(0x0.ffffffp-126f == 0x1p-126f);
static_assert(0x0.fffffeffffffp-126f == 0x0.fffffep-126f);
static_assert(0x1p-1075 == 0.0);
static_assert(0x3p-1075 == 0x1p-1073);
static_assert(0x0.fffffffffffff7ffffp-1022 == 0x0.fffffffffffffp-1022);
static_assert(0x0.fffffffffffff7ffffffp-1022 == 0x0.fffffffffffffp-1022);
static_assert(0x0.fffffffffffff8p-1022 == 0x1p-1022);
