//type:fp
//options: -A --c++20

void f();

constexpr bool b = noexcept(noexcept(f()));

static_assert(b);

//cwg: 3128
//title: Potentially-throwing unevaluated operands
//meeting: Croydon 3/26
//edg_status: Passes
