const int global=3;
constexpr const int& known(){return global;}
const int& reference=known();
static_assert(reference==3);
