#include <optional>
#include <variant>
#include <tuple>
#include <any>
#include <functional>
#include <span>
#include <string>
#include <string_view>

int main() {
  std::optional<std::string> optional;
  if (optional) return 1;
  optional.emplace(40, 'x');
  auto copied = optional;
  optional.reset();
  if (optional || copied->size() != 40) return 2;
  std::variant<int, std::string> variant(19);
  if (std::get<int>(variant) != 19) return 3;
  variant.emplace<std::string>("abc");
  if (std::get<std::string>(variant) != "abc" || variant.index() != 1) return 4;
  int size = std::visit([](const auto& v) -> int {
    if constexpr (std::is_same_v<std::decay_t<decltype(v)>, int>) return v;
    else return static_cast<int>(v.size());
  }, variant);
  if (size != 3) return 5;
  auto tuple = std::make_tuple(7, 11, 13);
  if (std::apply([](int a, int b, int c) { return a + b + c; }, tuple) != 31) return 6;
  std::any any = std::string(60, 'a');
  auto any_copy = any;
  any.reset();
  if (std::any_cast<const std::string&>(any_copy).size() != 60) return 7;
  std::function<int(int)> function = [offset = 19](int value) { return offset + value; };
  auto function_copy = function;
  function = nullptr;
  if (function || function_copy(23) != 42) return 8;
  int buffer[]{2, 3, 5, 7};
  std::span<int> span(buffer);
  span.subspan(1, 2)[1] = 17;
  if (buffer[2] != 17 || span.size_bytes() != sizeof buffer) return 9;
  constexpr std::string_view view("alpha/beta");
  static_assert(view.substr(view.find('/') + 1) == "beta");
  return 0;
}
