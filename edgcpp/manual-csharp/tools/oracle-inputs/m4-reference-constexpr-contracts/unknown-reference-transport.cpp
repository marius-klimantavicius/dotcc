constexpr int ignore(const int&){return 1;}
constexpr int outer(const int& reference){static_assert(ignore(reference)==1);return 1;}
static_assert(outer(3)==1);
