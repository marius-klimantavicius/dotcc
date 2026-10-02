#include <vector>
#include <stdexcept>
struct Item {
  static int alive;
  int value;
  explicit Item(int n) : value(n) { ++alive; }
  Item(const Item& other) : value(other.value) { ++alive; }
  Item(Item&& other) noexcept : value(other.value) { other.value = 0; ++alive; }
  ~Item() { --alive; }
};
int Item::alive;
int main() {
  {
    std::vector<Item> values;
    for (int i = 0; i != 40; ++i) values.emplace_back(i);
    if (values.size() != 40 || values[29].value != 29 || Item::alive != 40) return 1;
    bool caught = false;
    try { (void)values.at(90); } catch (const std::out_of_range&) { caught = true; }
    if (!caught) return 2;
    values.pop_back();
    if (Item::alive != 39) return 3;
  }
  return Item::alive == 0 ? 0 : 4;
}
