//remark:Explicit-this member functions
//options:--c++23;fp

constexpr auto lm = [](this auto &self, int i)->int {
                      return i>1 ? i*self(i-1) : 1;
                    };
static_assert(lm(9) == 362880);
