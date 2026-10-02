//type:fp
//options::-DNEG;fn
//options_all:--microsoft_v=2000 --parse --set_flag coroutines -tused
//fixing_pr:22110

namespace std {
    template <class...>
    using void_t = void;
 
    namespace experimental {
        template <class _Ret, class = void>
        struct _Coroutine_traits_sfinae {};
 
        template <class _Ret>
        struct _Coroutine_traits_sfinae<_Ret, void_t<typename _Ret::promise_type>> {
            using promise_type = typename _Ret::promise_type;
        };
 
        template <typename _Ret, typename... _Ts>
        struct coroutine_traits : _Coroutine_traits_sfinae<_Ret> {};

#ifndef NEG
	template <class Promise = void>
	  struct coroutine_handle {};
#endif /* NEG */
    }
}
 
template <typename T>
struct generator {
    struct promise_type {};
};
 
template <typename T>
generator<int> f(T) {
  co_yield 0;
  co_return 0;
}
