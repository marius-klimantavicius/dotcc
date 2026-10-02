//type:fp
//options: -A --c++20

#define F(X) X

#if __has_include(F(<#.h>))
#endif

//cwg: 3125
//title: Token convertibility requirement in #if
//meeting: Croydon 3/26
