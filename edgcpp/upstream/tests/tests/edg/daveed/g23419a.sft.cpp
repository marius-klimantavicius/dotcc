//remark:Defaulted spaceship operators
//options:--c++20;fn

#include <compare>

union S {
    int m;
    friend auto operator<=>(const S&, const S&&) = default;
};
