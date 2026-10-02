//type:fn
//options::--gnu_version 30400;fp:--gnu_version 40500:--clang_version 30200;fp:--clang_version 50000:--microsoft;fp
//options_all:--c++

struct S { S(){} } s1 = S::S();

S s2 = S::S();
S s3 = ::S();

