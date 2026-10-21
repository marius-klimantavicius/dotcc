constexpr int later();
const int value=later();
constexpr int later(){return 3;}
static_assert(value==3);
