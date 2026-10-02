//type:fp
//options:--c++11
//options_all:-A -tused

struct BB
{ };

struct B : BB
{ };

struct D : B
{ };


int f(B &);
int f(BB) = delete;

int g(B);
int g(BB &&) = delete;

D d;

auto vf = f(d);
auto vg = g(d);

//cwg: 3057
//title: Ranking of derived-to-base conversions should ignore reference binding
//meeting: Kona 11/25
//edg_status: Passes
