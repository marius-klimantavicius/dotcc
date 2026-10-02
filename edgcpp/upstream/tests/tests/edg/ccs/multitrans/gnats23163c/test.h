constexpr int fac[] = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

template<int N>
class count_time {
  static constexpr int factor = fac[N];

public:
  static constexpr double toDouble(int val) {
    return static_cast<double>(val * factor);
  }
};

constexpr double func(int val) {
  return count_time<9>::toDouble(1);
}
