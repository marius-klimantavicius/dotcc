//remark:if consteval
//options:--c++23;fp

int f() {
  if not consteval {
    return 3;
  }
}
