//type:cp
//options:--c++11:--c++17
//options_all:-tused --microsoft

template<typename T>
decltype(__is_trivially_copy_assignable(T)) test();

template<typename T>
decltype(__is_assignable(T, T)) test2();

bool a = test<int>();
bool b = test2<int>();
