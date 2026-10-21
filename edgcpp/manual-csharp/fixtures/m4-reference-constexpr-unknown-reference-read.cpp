constexpr int outer(const int& reference){static_assert(reference==3);return 1;}
static_assert(outer(3)==1);
