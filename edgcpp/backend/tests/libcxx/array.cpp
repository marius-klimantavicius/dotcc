#include <array>
constexpr std::array<int, 3> fixed{1, 2, 3};
static_assert(fixed[2] == 3 && fixed.size() == 3);
int main() {
  std::array<int, 3> a{3, 5, 7}, b{0, 0, 0};
  a.swap(b);
  int sum = 0;
  for (int value : b) sum += value;
  return sum == 15 && a.front() == 0 && b.back() == 7 ? 0 : 1;
}
