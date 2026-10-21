int global=3;
constexpr int& alias=global;
static_assert(alias==3);
