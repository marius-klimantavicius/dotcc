constexpr int parameter(int value){const int& alias=value;const int next=alias+1;return next;}
constexpr int temporary(){const int& alias=3;return alias;}
static_assert(parameter(4)==5);
static_assert(temporary()==3);
