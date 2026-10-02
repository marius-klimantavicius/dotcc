//type:fn
//options:--c++17:--c++26
//options_all:-A -tused

int g();

template<int ... I>
int i = g(decltype(I)::y ...);

//cwg: 3095
//title: Type-dependent packs that are not structured binding packs
//meeting: Kona 11/25
//edg_status: Passes
