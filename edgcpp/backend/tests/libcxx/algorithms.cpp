#include <algorithm>
#include <numeric>
#include <array>
#include <iterator>

struct Entry { int key; int order; };
int main() {
  std::array<int, 8> values{8, 3, 2, 7, 1, 5, 4, 6};
  std::sort(values.begin(), values.end());
  for (int i = 0; i < 8; ++i) if (values[i] != i + 1) return 1;
  if (std::accumulate(values.begin(), values.end(), 0) != 36) return 2;
  std::array<int, 8> squares{};
  std::transform(values.begin(), values.end(), squares.begin(), [](int i) { return i * i; });
  if (squares[7] != 64 || std::inner_product(values.begin(), values.end(), values.begin(), 0) != 204) return 3;
  Entry stable[]{{2, 0}, {1, 1}, {2, 2}, {1, 3}, {2, 4}};
  std::stable_sort(std::begin(stable), std::end(stable), [](Entry a, Entry b) { return a.key < b.key; });
  if (stable[0].order != 1 || stable[1].order != 3 || stable[4].order != 4) return 4;
  std::reverse(values.begin(), values.end());
  std::nth_element(values.begin(), values.begin() + 3, values.end());
  if (values[3] != 4) return 5;
  std::make_heap(values.begin(), values.end());
  if (values.front() != 8) return 6;
  std::sort_heap(values.begin(), values.end());
  if (!std::is_sorted(values.begin(), values.end())) return 7;
  auto midpoint = std::stable_partition(values.begin(), values.end(), [](int i) { return i % 2 == 0; });
  if (midpoint - values.begin() != 4 || values[0] != 2 || values[4] != 1) return 8;
  std::array<int, 4> prefix{};
  std::inclusive_scan(values.begin(), values.begin() + 4, prefix.begin());
  return prefix[3] == 20 ? 0 : 9;
}
