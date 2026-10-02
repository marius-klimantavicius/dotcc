//remark:Class constant template arguments
//options:--c++20;fp

struct Const
{
int operator()() const { return 0; }
};

struct NoConst
{
int operator()() { return 0; }
};

template<auto f>
inline constexpr bool is_invocable_v = requires { f(); };

static_assert(is_invocable_v<Const{}>); // Ok
static_assert(!is_invocable_v<NoConst{}>); // Error 

