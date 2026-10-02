#if defined(DIFF)
constexpr int fac[] = {10, 11, 12, 13, 14, 15, 16, 17, 18, 9, 20};
#elif defined(DIFF2)
constexpr int fac2[] = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
#else
constexpr int fac[] = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
#endif


template<int N, typename base = int>
class count_time {
private:
#if defined(DIFF2)
  static constexpr int factor = fac2[N];
#else
  static constexpr int factor = fac[N];
#endif

public:
  using baseType = base;
  static constexpr double toDouble(baseType val) {
    return static_cast<double>(val * factor);
  }
};

template<class Tconv>
class TimeRepresentation {
public:
  using baseType = typename Tconv::baseType;
  TimeRepresentation(); // = default;
  ~TimeRepresentation(); // = default;

  constexpr explicit TimeRepresentation(int /*unused*/)
  : internalTimeCode(0)
  {}
private:
  baseType internalTimeCode;
};

using Time = TimeRepresentation<count_time<9>>;

struct t {
  Time grantedTime;
  int x;
};
