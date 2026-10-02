//type:fn
//options:--c++26 -A

#define FOO 1
#if __has_embed(__FILE__ prefix(defined(FOO),) suffix(,defined(FOO)))
#endif
