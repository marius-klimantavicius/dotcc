// The exact allocation and exception class closure needed by the EDG compiler.
#include <new>
#include <exception>
#include <cstring>
static int alive;
struct Item {
  int value;
  Item() : value(19) { ++alive; }
  ~Item() { --alive; }
};
static void handler() { throw 47; }
int main() {
  Item* one = new Item;
  Item* many = new Item[9];
  if (alive != 10 || one->value != 19 || many[8].value != 19) return 1;
  delete one;
  delete[] many;
  if (alive) return 2;
  int* optional = new (std::nothrow) int(29);
  if (!optional || *optional != 29) return 3;
  delete optional;
  void* sized = ::operator new(27);
  ::operator delete(sized, static_cast<std::size_t>(27));
  auto old = std::set_new_handler(handler);
  if (std::get_new_handler() != handler || std::set_new_handler(old) != handler) return 4;
  try { Item during; throw std::bad_alloc(); }
  catch (const std::exception& e) {
    if (alive || std::strcmp(e.what(), "std::bad_alloc")) return 5;
  }
  try { throw std::bad_exception(); }
  catch (const std::exception& e) {
    if (std::strcmp(e.what(), "std::bad_exception")) return 6;
  }
  volatile unsigned long excessive = ~0UL;
  try { int* impossible = new int[excessive]; delete[] impossible; return 7; }
  catch (const std::bad_array_new_length&) { if (alive) return 8; }
  std::exception_ptr pending;
  try { throw std::bad_alloc(); }
  catch (...) { pending = std::current_exception(); }
  try { std::rethrow_exception(pending); return 9; }
  catch (const std::bad_alloc& e) {
    if (std::strcmp(e.what(), "std::bad_alloc")) return 10;
  }
  auto saved = std::set_new_handler(handler);
  try { (void)::operator new(excessive); return 11; }
  catch (int value) { if (value != 47) return 12; }
  if (::operator new(excessive, std::nothrow) != nullptr) return 13;
  std::set_new_handler(nullptr);
  try { (void)::operator new(excessive); return 14; }
  catch (const std::bad_alloc&) {}
  std::set_new_handler(saved);
  return 0;
}
