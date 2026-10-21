constexpr const int& stable=3;
constexpr const int& alias=stable;
static_assert(stable==3 && alias==3);
