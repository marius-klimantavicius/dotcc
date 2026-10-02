//remark:Nested lambdas
//options:--microsoft_v=1903;fp

template<typename F >
void for_each(F f)
{
    f(1);
}
void combine_surface(int dst){
    for_each([&](auto op) {
        for_each([&](auto op) {
            for_each([&](auto op) {
                (void)dst;
            });
        });
    });
}

