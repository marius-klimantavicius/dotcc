namespace std::meta { using info = decltype(^^void); }

template<int N> consteval auto retained_token_pattern() {
  return ^^{ int generated_value = \val(N); };
}
