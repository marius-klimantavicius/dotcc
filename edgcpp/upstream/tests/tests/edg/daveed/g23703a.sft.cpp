//remark:Variable instantiations in SFINAE
//options:--c++14;fp

void foo()
{
    auto l = [] (auto b) {
                 return [](auto d) -> decltype(b + d) { return 0; };
             };

    int (*fp) (int) = l(1);
}

