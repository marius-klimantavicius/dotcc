//type:fp
//options:--c++26 -A --embed_dir=.

#define Test_name nosuch
#if !__has_embed(<Test_name.c>)
#error Test_name.c not found
#endif

#undef Test_name

#define EMPTY
#define X Test_name
#define Y c
#define Z 42

constexpr unsigned char arr1[] = {
#embed <Test_name.c> prefix(Z,)
};
static_assert(arr1[0] == Z);

constexpr unsigned char arr2[] = {
#embed EMPTY <X.Y>  prefix(Z,)
};
static_assert(arr2[0] == Z);

//cwg: 3015
//title: Handling of header-names for #include and #embed
//meeting: Sofia 6/25
//edg_status: Passes
