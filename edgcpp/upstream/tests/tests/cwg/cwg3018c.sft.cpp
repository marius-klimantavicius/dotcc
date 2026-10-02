//type:fp
//options:--c++26 -A

#define FOO 42
constexpr unsigned char defined(unsigned char i)
{
  return i;
}

constexpr unsigned char arr[] = {
#embed __FILE__ prefix(defined(FOO),) suffix(,defined(FOO))
};

static_assert(arr[0] == 42);
static_assert(arr[sizeof(arr) - 1] == 42);
