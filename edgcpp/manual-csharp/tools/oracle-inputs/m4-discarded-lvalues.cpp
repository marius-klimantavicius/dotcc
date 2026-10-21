// Discarding a nonvolatile lvalue/function designator does not read storage.
int runtime_only();
constexpr int discarded() { int value; value; (value); runtime_only; return 1; }
static_assert(discarded() == 1);
