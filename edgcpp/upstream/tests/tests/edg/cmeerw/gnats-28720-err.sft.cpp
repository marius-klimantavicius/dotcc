//type:fn
//options:--c++20 --clang_version 220100

template<typename, int>
int v1;

template<typename, template<typename> class>
int v2;

template<typename ... Ts>
int i = v1<__builtin_dedup_pack<Ts ...> ...> +
        v2<__builtin_dedup_pack<Ts ...> ...>;

int j = i<int, char>;
