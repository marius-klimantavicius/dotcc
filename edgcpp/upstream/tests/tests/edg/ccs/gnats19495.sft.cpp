//type:cp
//options::--g++:--clang:--microsoft
//options_all:--c++17 --no_exceptions

template<typename _Signature> struct _Mem_fn_traits;

template<typename _Res, typename _Class, typename ... _ArgTypes>
struct _Mem_fn_traits<_Res (_Class:: *)(_ArgTypes ...)  > {};

template<typename _Res, typename _Class, typename ... _ArgTypes>
struct _Mem_fn_traits<_Res (_Class:: *)(_ArgTypes ...)  noexcept> {};
