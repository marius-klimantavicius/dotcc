// Mutable static storage cannot become constant through initializer folding.
int value = 3;
constexpr int read_value() { return value; }
static_assert(read_value() == 3);
