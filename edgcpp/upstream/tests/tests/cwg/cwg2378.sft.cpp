//type:fp
//options_all:--c++20 -tused -A

template <typename... T>
void foo(T&... ts) {
    [&...us=ts]{};
}

//cwg: 2378
//title: Inconsistent grammar for reference init-capture of pack
//meeting: Prague 02/20
//edg_status: EDGcpfe/20024
//fixed_in: 6.1
