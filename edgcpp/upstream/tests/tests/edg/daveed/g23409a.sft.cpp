//remark:Comparison category types
//options:--c++20;fp

#include <compare>
const char* to_str(std::strong_ordering v) {
    if (v == std::strong_ordering::equal)
        return "equal";
    else if (v == std::strong_ordering::less)
        return "less";
    else if (v == std::strong_ordering::greater)
        return "greater";
    else
        return "unknown";
}
