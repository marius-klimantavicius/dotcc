//remark:if consteval
//options:--c++23;fp

constexpr bool test() {
  if consteval { return true; }
  return false;
}
static_assert(test());

constexpr bool anti() {
  if not consteval {
    return false;
  } else {
    return true;
  }
}
static_assert(anti());

