//type:fp
//options:--c++17:--c++26
//options_all:-A -tused

struct B
{
  int i;
  long l;
};

struct R
{
  B b{};

  const B *begin() const;
  const B *end() const;
};

void f()
{
  for (static auto [ a, b ] : R())
  { }
}

//cwg: 3085
//title: Apply restriction inside for-range-declaration
//meeting: Kona 11/25
//edg_status: Passes
