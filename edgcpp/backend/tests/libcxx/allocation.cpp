#include <new>
#include <typeinfo>
#include <cstring>
#include <cstdint>

static int alive;
struct Item {
  int value;
  Item() : value(19) { ++alive; }
  ~Item() { --alive; }
};
struct alignas(128) Aligned { unsigned long words[17]; };
struct Base { virtual ~Base() = default; };
struct Derived : Base { int value = 31; };
struct Other : Base {};
static void handler() { throw 47; }

int main() {
  Item* one = new Item;
  Item* many = new Item[9];
  if (alive != 10 || one->value != 19 || many[8].value != 19) return 1;
  delete one;
  delete[] many;
  if (alive) return 2;
  Aligned* aligned = new Aligned{};
  if (reinterpret_cast<std::uintptr_t>(aligned) % alignof(Aligned) || aligned->words[16]) return 3;
  aligned->words[16] = 83;
  delete aligned;
  int* optional = new (std::nothrow) int(29);
  if (!optional || *optional != 29) return 4;
  delete optional;
  auto old = std::set_new_handler(handler);
  if (std::get_new_handler() != handler || std::set_new_handler(old) != handler) return 5;
  Derived derived;
  Base* base = &derived;
  if (dynamic_cast<Derived*>(base) != &derived || dynamic_cast<Other*>(base)) return 6;
  try { (void)dynamic_cast<Other&>(*base); return 7; }
  catch (const std::bad_cast& e) { if (!*e.what()) return 8; }
  Base* empty = nullptr;
  try { (void)typeid(*empty); return 9; }
  catch (const std::bad_typeid& e) { if (!*e.what()) return 10; }
  if (!(typeid(*base) == typeid(Derived)) || typeid(*base) == typeid(Base)) return 11;
  try { throw std::bad_alloc(); }
  catch (const std::exception& e) { if (std::strcmp(e.what(), "std::bad_alloc")) return 12; }
  return 0;
}
