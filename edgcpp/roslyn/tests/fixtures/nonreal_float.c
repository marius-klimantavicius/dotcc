/* Real C99 non-real literals, backing expressions and lowered initialization. */
float _Complex complex_float = 1.25f + 2.5f * __I__;
double _Complex complex_double = -3.5 + 4.25 * __I__;
long double _Complex complex_long_double = 0.125L - 0.75L * __I__;
#if !defined(__GNUC__)
float _Imaginary imaginary_float = 0.5f * __I__;
double _Imaginary imaginary_double = -0.25 * __I__;
long double _Imaginary imaginary_long_double = 0.75L * __I__;
#endif

double _Complex complex_identity(double _Complex input) {
  return input + complex_double;
}

#if !defined(__GNUC__)
double _Imaginary imaginary_identity(double _Imaginary input) {
  return input + imaginary_double;
}
#endif
