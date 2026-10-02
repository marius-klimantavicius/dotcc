//remark:if consteval
//options:--c++23;fp

consteval int f() {
  if not consteval {
    return 3;
  }
}
