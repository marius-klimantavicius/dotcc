//remark:if consteval
//options:--c++23;fp

constexpr int f(int p) {
  if consteval {
    p = -p;
  } else {
    p = 2*p;
  }
  if !consteval {
    p = 2*p;
  }
  if not consteval {
    return p > 3;
  } else {
    return p < 0;
  }
}

