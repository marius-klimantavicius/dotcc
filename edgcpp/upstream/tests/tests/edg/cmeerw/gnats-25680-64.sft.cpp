//type:fp
//options:--c++11
//options_all:--clang --target linux_x86_64

template<unsigned N>
struct C
{
  char arr[N];
};

static_assert(sizeof(_Atomic C<5>) == 8, "sizeof");
static_assert(alignof(_Atomic C<5>) == 8, "alignof");

static_assert(sizeof(_Atomic C<9>) == 16, "sizeof");
static_assert(alignof(_Atomic C<9>) == 16, "alignof");

static_assert(sizeof(_Atomic C<17>) == 17, "sizeof");
static_assert(alignof(_Atomic C<17>) == 1, "alignof");
