//type:fp
//options:--c++11
//options_all:--clang --target linux_i686

template<unsigned N>
struct C
{
  char arr[N];
};

static_assert(sizeof(_Atomic C<5>) == 8, "sizeof");
static_assert(alignof(_Atomic C<5>) == 8, "alignof");

static_assert(sizeof(_Atomic C<9>) == 9, "sizeof");
static_assert(alignof(_Atomic C<9>) == 1, "alignof");
