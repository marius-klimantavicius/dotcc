//options_all:--c++20
template <class _Ty>
_Ty &&declval() noexcept;

template <class _Ty>
struct _Choice_t {
    _Ty _Strategy  = _Ty{};
    bool _No_throw = false;
};

void iter_move();

template <class _Ty>
concept _Has_ADL = requires(_Ty&& __t) {
    iter_move(static_cast<_Ty&&>(__t));
};

template <class _Ty>
concept _Can_deref = requires(_Ty&& __t) {
    *static_cast<_Ty&&>(__t);
};

class _Cpo {
private:
    enum class _St { _None, _Custom, _Fallback };

    template <class _Ty>
    static constexpr _Choice_t<_St> _Choose() noexcept {
        (void)_Has_ADL<_Ty>;
        return {_St::_Fallback, noexcept(*declval<_Ty>())};
    }

    template <class _Ty>
    static constexpr _Choice_t<_St> _Choice = _Choose<_Ty>();

public:
    template <class _Ty> requires (_Choice<_Ty>._Strategy != _St::_None)
    constexpr void operator()(_Ty &&_Val) const noexcept(_Choice<_Ty>._No_throw);
} iter_move2;

struct _Vector_const_iterator {
     const int& operator*() const;
};

void f(_Vector_const_iterator __i) {
     iter_move2(__i);
}
