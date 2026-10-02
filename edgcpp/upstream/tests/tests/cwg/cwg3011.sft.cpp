//type:fn
//options:--c++20
//options_all:-A

struct A
{
  A(int);
};

void f(int n)
{
  new A[3](1, 2, 3);            // OK
  new A[4](1, 2, 3);            // error
  new A[n](1, 2, 3);            // error
}

constexpr bool g(int n)
{
  auto *p = new char[n]('a', 'b');
  delete[] p;

  return true;
}

static_assert(g(1));            // error
static_assert(g(2));            // OK

//cwg: 3011
//title: Parenthesized aggregate initialization for new-expressions
//meeting: Kona 11/25
//edg_status: Passes
