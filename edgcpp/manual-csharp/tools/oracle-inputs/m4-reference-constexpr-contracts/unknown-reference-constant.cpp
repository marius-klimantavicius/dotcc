constexpr int outer(const int& parameter){constexpr const int& alias=parameter;return 1;}
static_assert(outer(3)==1);
