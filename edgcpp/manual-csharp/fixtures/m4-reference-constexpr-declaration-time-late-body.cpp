const int global=3;
constexpr const int& later();
const int& reference=later();
constexpr const int& later(){return global;}
static_assert(reference==3);
