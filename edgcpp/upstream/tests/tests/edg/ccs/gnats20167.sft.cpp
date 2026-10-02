//type:cp
//options::--g++:--clang
//options_all:--c++17

struct S { S(); S(const S&) = delete; ~S(); };
 
S foo(S);
S s1 = foo(S{});
S s2 = (42, S{});
S s3 = (54, foo(S{}));
S s4 = (S{}, S{});

