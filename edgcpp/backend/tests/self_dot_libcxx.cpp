// Exercise the actual libc++ facilities consumed by dot_gen_be.
#include <algorithm>
#include <cstdio>
#include <limits>
#include <map>
#include <set>
#include <stdexcept>
#include <string>
#include <vector>

static std::map<const void*, std::string> names;
static std::set<std::string> unique_names;
static std::string generated;

int main() {
  std::vector<std::pair<int, std::string>> values;
  for (int i = 0; i < 80; ++i) {
    std::string text = "long string for allocated payload " + std::to_string(i);
    values.push_back({i % 3, text});
    unique_names.insert(text);
  }
  std::stable_sort(values.begin(), values.end(), [](const auto& a, const auto& b) {
    return a.first < b.first;
  });
  for (auto& value : values) names[&value] = value.second;
  if (names.size() != 80 || unique_names.size() != 80) return 1;
  if (values.front().second != "long string for allocated payload 0" ||
      values.back().second != "long string for allocated payload 77") return 2;

  generated.assign(4096, 'a');
  generated.replace(100, 5, "replacement");
  generated.insert(200, "inserted");
  generated.erase(300, 7);
  if (generated.find("replacement") != 100 || generated.size() != 4103) return 3;
  std::string moved = std::move(generated);
  if (moved.substr(100, 11) != "replacement") return 4;
  if (std::to_string(std::numeric_limits<long long>::min()) != "-9223372036854775808" ||
      std::to_string(std::numeric_limits<unsigned long long>::max()) != "18446744073709551615") return 5;
  try { (void)moved.at(moved.size()); return 6; }
  catch (const std::out_of_range& error) { if (!error.what()[0]) return 7; }
  try { moved.resize(moved.max_size() + 1); return 8; }
  catch (const std::length_error& error) { if (!error.what()[0]) return 9; }
  names.clear();
  unique_names.erase("long string for allocated payload 20");
  std::printf("%zu %zu %zu %s\n", names.size(), unique_names.size(), moved.size(),
              values.back().second.c_str());
  return 0;
}
