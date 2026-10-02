//type:rp
//options::--gnu_version 70300:--clang_version 90000
//options_all:--c++14

extern "C" int printf(const char *, ...);

template <typename _Tp>
class allocator
{
 public:
  allocator() {
    printf("allocator()\n");
  }
  ~allocator() {
    printf("~allocator()\n");
  }
};

template <typename _CharT, typename _Alloc>
  class basic_string
{
  _Alloc _M_dataplus;
 public:
  basic_string(const _CharT *, const _Alloc &p2 = _Alloc ()) {
    printf("basic_string()\n");
  }
  ~basic_string() {
    printf("~basic_string()\n");
  }
};

struct Foo
{
  basic_string<char, allocator <char>> separators = "";
  /* No ctor here as having one avoids the bug. */
  ~Foo() {
    printf("~Foo()\n");
  }
};

struct Bar
{
  Foo foo = { };
  /* No ctor here as having one avoids the bug. */
  ~Bar() {
    printf("~Bar()\n");
  }
};

void test (const Bar & = {})
{
}

int main() {
  test();
}
