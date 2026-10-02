//type: fp
//options: 
# 1 "SemaCXX/warn-deprecated-specializations-in-system-headers.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-deprecated-specializations-in-system-headers.cpp" 2
# 23 "SemaCXX/warn-deprecated-specializations-in-system-headers.cpp"
# 1 "SemaCXX/warn-deprecated-specializations-in-system-headers.cpp" 1
# 5 "SemaCXX/warn-deprecated-specializations-in-system-headers.cpp" 3

template <typename T>
struct traits;

template <>
struct [[deprecated]] traits<int> {};

template<typename T, typename Trait = traits<T>>
struct basic_string {};


using __do_what_i_say_not_what_i_do = traits<int> ;

template<typename T, typename Trait = traits<double>>
struct should_not_warn {};
# 24 "SemaCXX/warn-deprecated-specializations-in-system-headers.cpp" 2

basic_string<int> test1;
should_not_warn<int> test2;
