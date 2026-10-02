#include <utility>
struct MoveOnly {
  int value;
  explicit MoveOnly(int n) : value(n) {}
  MoveOnly(MoveOnly&& other) noexcept : value(other.value) { other.value = 0; }
  MoveOnly& operator=(MoveOnly&& other) noexcept { value = other.value; other.value = 0; return *this; }
  MoveOnly(const MoveOnly&) = delete;
};
int main() {
  int a = 3, b = 5;
  std::swap(a, b);
  std::pair<int, int> pair(a, b);
  MoveOnly original(19), moved(std::move(original));
  int old = std::exchange(a, 11);
  return pair.first == 5 && pair.second == 3 && old == 5 && a == 11 &&
         moved.value == 19 && original.value == 0 ? 0 : 1;
}
