#include <exception>
struct Payload { int value; };
int main() {
  std::exception_ptr saved;
  try { throw Payload{37}; } catch (...) { saved = std::current_exception(); }
  if (!saved) return 1;
  auto copy = saved;
  saved = nullptr;
  try { std::rethrow_exception(copy); }
  catch (const Payload& value) { return value.value == 37 ? 0 : 2; }
  return 3;
}
