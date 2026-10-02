//type:rp
//options:--c++26
//options_all:-A -tused

extern "C" int printf(const char *, ...);

struct B
{
  B(int v)
    : i(v)
  { }

  ~B()
  {
    i = -1;
  }

  int i;
};

struct A
{
  B m;

  const B &b()
  {
    return m;
  }
};

// expected output:
// 1
// 2
int main()
{
  int i = 0;

  template for (const int & v : { A{ 1 }.b().i, A{ 2 }.b().i })
  {
    printf("%d\n", v);
    i += v;
  }

  return i == 3 ? 0 : 1;
}

//cwg: 3043
//title: Lifetime extension for temporaries in expansion statements
//meeting: Kona 11/25
//edg_status: EDGcpfe/28537
