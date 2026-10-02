// Empty function packs leave non-runtime placeholders in EDG's scope lists.
template<class... Types> constexpr int count(Types... args) { return sizeof...(args); }
template<class... Types> int total(int first, Types... args) { return (first + ... + args); }
template<class... Types> int forward(Types... args) { return total(0, args...); }
template<int... Values> constexpr int constant_total() { return (0 + ... + Values); }
struct Capture {
  int value;
  template<class... Types> Capture(Types... args) : value((0 + ... + args)) {}
};
extern int unbound_only_in_unused_inline();
inline int unused_inline() { return unbound_only_in_unused_inline(); }
static_assert(count() == 0 && count(1, 2L) == 2);
static_assert(constant_total<>() == 0 && constant_total<2, 3, 4>() == 9);
int main() {
  Capture empty, populated(3, 5);
  return count() == 0 && total(7) == 7 && forward() == 0 &&
         forward(1, 2, 3) == 6 && empty.value == 0 && populated.value == 8 ? 0 : 1;
}
