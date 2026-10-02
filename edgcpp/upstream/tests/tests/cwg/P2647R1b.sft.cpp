//type:fp
//options_all:--c++20 -tused -A
constexpr char xdigit(int n) {
  static constexpr char digits[] = "0123456789abcdef";
  return digits[n];
}
