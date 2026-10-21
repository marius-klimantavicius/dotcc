// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored native-only proposal; no managed frontend acceptance is asserted.
// Proposed profile: strict C++20, no code generation, used instantiation.
template<class T> struct floating_kind;
template<> struct floating_kind<float> { static constexpr int value = 1; };
template<> struct floating_kind<double> { static constexpr int value = 2; };
template<> struct floating_kind<long double> { static constexpr int value = 3; };
static_assert(sizeof(float) == 4 && sizeof(double) == 8);
static_assert(floating_kind<decltype(1.0f)>::value == 1);
static_assert(floating_kind<decltype(1.0)>::value == 2);
static_assert(floating_kind<decltype(1.0L)>::value == 3);
static_assert(16777217.0f == 16777216.0f);
static_assert(16777219.0f == 16777220.0f);
static_assert(9007199254740993.0 == 9007199254740992.0);
static_assert(9007199254740995.0 == 9007199254740996.0);
static_assert(0x1p-149f > 0.0f && 0x1p-126f > 0x1p-149f);
static_assert(0x1p-1074 > 0.0 && 0x1p-1022 > 0x1p-1074);
