constexpr long double negative_zero_long = -0.0L;
constexpr float negative_zero_float = -0.0f;
constexpr double negative_zero_double = -0.0;
constexpr long double extended_finite = 0x1.0000000000000002p0L;
constexpr __float128 wide_finite = 0x1.0000000000000000000000000001p0Q;
constexpr _Float16 narrow_finite = 0x1.004p0f16;
constexpr _Float16 narrow_negative_zero = -0.0f16;
#if defined(__STDCPP_FLOAT32_T__)
constexpr auto standard_float32 = 1.25f32;
#endif
#if defined(__STDCPP_BFLOAT16_T__)
constexpr auto standard_bfloat16 = 1.25bf16;
#endif
