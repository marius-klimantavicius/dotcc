static constexpr int fac(int N) {
  return N;
}

template<int N, typename base = int>
class count_time {
private:
  static constexpr int factor = fac(N);

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
