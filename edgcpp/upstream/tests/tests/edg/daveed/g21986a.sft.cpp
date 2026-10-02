//remark:Explicit specialization of direct-initialized variable template
//options:--c++17;fp

#include <initializer_list>
template <typename T> constexpr auto& v{""};
template <> inline constexpr auto& v<bool>{"b"};
