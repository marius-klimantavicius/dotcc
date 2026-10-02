#include <memory>
struct Item {
  static int alive;
  int value;
  explicit Item(int n) : value(n) { ++alive; }
  ~Item() { --alive; }
};
int Item::alive;
int main() {
  {
    auto unique = std::make_unique<Item>(7);
    auto shared = std::make_shared<Item>(19);
    std::weak_ptr<Item> weak(shared);
    auto locked = weak.lock();
    if (Item::alive != 2 || unique->value != 7 || locked->value != 19 || shared.use_count() != 2) return 1;
    shared.reset();
    if (weak.expired()) return 2;
    locked.reset();
    if (!weak.expired() || Item::alive != 1) return 3;
  }
  return Item::alive == 0 ? 0 : 4;
}
