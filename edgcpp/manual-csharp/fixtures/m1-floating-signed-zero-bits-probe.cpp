// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Native builtin/provenance probe: availability and strict-profile acceptance
// of __builtin_bit_cast are unqualified until supervisor execution.
static_assert(sizeof(unsigned int) == sizeof(float));
static_assert(sizeof(unsigned long long) == sizeof(double));
static_assert(__builtin_bit_cast(unsigned int, -0.0f) == 0x80000000U);
static_assert(__builtin_bit_cast(unsigned int, -0.0f * 2.0f) == 0x80000000U);
static_assert(__builtin_bit_cast(unsigned int, -0.0f + -0.0f) == 0x80000000U);
static_assert(__builtin_bit_cast(unsigned long long, static_cast<double>(-0.0f)) == 0x8000000000000000ULL);
