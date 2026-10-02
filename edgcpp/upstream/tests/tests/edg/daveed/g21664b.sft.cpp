//remark:__is_constructible and incomplete types
//options:--c++17;fn:--c++17 --gnu=80000;fp:--c++17 --clang;fn

                struct S;
                bool bb = __is_destructible(S);
