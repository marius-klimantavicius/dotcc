 namespace std {
    template <class>
    struct coroutine_handle {};

    struct noop_coroutine_promise {};

    template <>
    struct coroutine_handle<noop_coroutine_promise> {
        operator coroutine_handle<void>();
    };
}
