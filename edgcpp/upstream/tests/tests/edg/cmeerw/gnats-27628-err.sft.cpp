//type:fn
//options:--c++11 --clang_version 190100

int i = (__builtin_operator_delete(1),
         __builtin_operator_new(nullptr),
         0);
