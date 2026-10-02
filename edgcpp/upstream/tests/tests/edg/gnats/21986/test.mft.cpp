//options_all:-I. --microsoft_version=1925 --ms_c++17 --no_ms_permissive
//type:fp
//source_files:initializer_list.stdh
#include <initializer_list>
template <typename T> constexpr auto& v{""};
template <> inline constexpr auto& v<bool>{"b"};
