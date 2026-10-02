//type:cp
//options_all:--c++17

template <int a> struct b { static const int c = a; };
class f;
template <class d> struct g : b<__is_trivially_assignable(f, d)> {};
class f {
    template <typename e>
        void operator=(e) noexcept(b<__is_nothrow_assignable(int, int)>::c);
};
static_assert(g<f>::c);
