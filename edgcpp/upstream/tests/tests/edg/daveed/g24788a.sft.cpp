//remark:SFINAE
//options:--gnu=80300 --c++17;fp

template<bool> struct EnableIf {};
template<> struct EnableIf<true> {
  using Type = int;
};

template<typename To, typename From>
constexpr bool check_conv() {
  return sizeof(To) == sizeof(From);
}

template<typename To, typename From,
         typename EnableIf<bool(check_conv<To, From>())>::Type = 0>
To conv(From x) {
  union { From from; To to; } converter = { x };
  return converter.to;
}

long r = conv<long>(1.2);

