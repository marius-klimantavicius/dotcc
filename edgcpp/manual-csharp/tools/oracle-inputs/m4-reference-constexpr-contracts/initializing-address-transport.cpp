constexpr int ignore(const int&){return 1;}
const int value=ignore(value);
static_assert(value==1);
