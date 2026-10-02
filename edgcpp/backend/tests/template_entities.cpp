// Dependent template entities have no runtime storage or callable body.
// EDG's C backend skips them with ignore_*_in_back_end predicates.
template<class T> constexpr bool fits_word = sizeof(T) <= sizeof(long);
template<class T> struct Traits {
  static constexpr bool small = fits_word<T>;
  static constexpr int bytes = sizeof(T);
};
template<class T> constexpr T twice(T value) { return value + value; }
template<class T> int count_calls() { static int count; return ++count; }

static_assert(fits_word<int> && !fits_word<long[2]>, "variable template");
static_assert(Traits<int>::small && Traits<long>::bytes == 8, "static member template");

int main() {
  return twice(7) == 14 && count_calls<int>() == 1 &&
         count_calls<long>() == 1 && count_calls<int>() == 2 ? 0 : 1;
}
