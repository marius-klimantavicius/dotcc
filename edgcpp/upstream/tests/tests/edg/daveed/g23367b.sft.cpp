//remark:Deducing bool templ param from noexcept
//options:--gnu=80400 --c++17;fp

template<typename R, typename ...Ps, bool N>
  constexpr bool is_noexcept(R(&)(Ps...) noexcept(N)) { return N; }
void g() noexcept;
static_assert(is_noexcept(g));

