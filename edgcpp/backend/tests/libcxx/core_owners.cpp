#include <vector>
#include <string>
#include <map>
#include <unordered_map>
#include <memory>
#include <algorithm>
#include <new>

static std::vector<int> history;
static std::string owner_name;
static std::map<int, std::string> tree;
static std::unordered_map<int, std::shared_ptr<std::string>> shared;
static thread_local std::string thread_name;
static thread_local int thread_step;
static void handler() { throw 71; }

extern "C" int core_owner_step(int owner, int step) {
  if (history.size() != static_cast<unsigned>(step - 1)) return 1;
  if (step == 1) {
    if (std::get_new_handler() != nullptr) return 2;
    std::set_new_handler(handler);
    owner_name.assign(80, static_cast<char>('a' + owner));
  }
  if (std::get_new_handler() != handler || owner_name.size() != 80 || owner_name[33] != 'a' + owner) return 3;
  history.push_back(owner * 1000 + step);
  tree[step] = owner_name;
  shared[step] = std::make_shared<std::string>(owner_name);
  if (history.front() != owner * 1000 + 1 || history.back() != owner * 1000 + step) return 4;
  if (tree.size() != history.size() || shared.size() != history.size()) return 5;
  auto copy = shared[step];
  if (copy.use_count() != 2 || *copy != tree[step]) return 6;
  auto sorted = history;
  std::reverse(sorted.begin(), sorted.end());
  std::sort(sorted.begin(), sorted.end());
  if (sorted != history) return 7;
  try { throw std::bad_alloc(); }
  catch (const std::exception& value) { if (!*value.what()) return 8; }
  return 0;
}
extern "C" int core_thread_step(int owner, int step) {
  if (++thread_step != step) return 11;
  if (step == 1) thread_name.assign(96, static_cast<char>('A' + owner));
  return thread_name.size() == 96 && thread_name[51] == 'A' + owner ? 0 : 12;
}
#ifdef NATIVE_LIBRARY_CORE
int main() {
  for (int i = 1; i <= 5; ++i) {
    if (int result = core_owner_step(1, i)) return result;
    if (int result = core_thread_step(1, i)) return result;
  }
  return 0;
}
#endif
