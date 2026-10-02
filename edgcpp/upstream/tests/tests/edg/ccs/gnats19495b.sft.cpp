//type:cp
//options_all:--microsoft_version 1913 --ms_c++latest --no_exceptions

template<class _Ret, class... _Types>
struct _Is_function {};

template<class _Ret, class... _Types>
struct _Is_function<_Ret (_Types...)>
{};

#if defined(__cpp_noexcept_function_type)
template<class _Ret, class... _Types>
struct _Is_function<_Ret (_Types...)   noexcept>
{};
#else
#error BADPATH
#endif
