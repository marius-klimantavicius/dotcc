//remark:Rewrites in SFINAE contexts
//options:--c++20;fp

struct Order { friend bool operator<(Order, decltype(nullptr)); };
struct S {};
Order operator<=>(S const&, S const&);
template<typename T> concept NotApplies = requires (T p) { !p; };
template<typename T> concept C = requires (T s) { { s < s } -> NotApplies; };
static_assert(C<S>);

