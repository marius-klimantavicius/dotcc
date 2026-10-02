//type:fp
//options:--c++26
//options_all:-A -tused

struct B0 { int b0; };

struct B {
  B &operator=(const B &) = default;
  int x;
};

struct D : B0, B {
  using B::operator=;
private:
  D &operator=(const D &) && = default;
};

struct Q {
  Q &operator=(const Q &) = default;
  D d;
};

constexpr bool f()
{
  Q q1{{{1}, {2}}};
  Q q2{{{3}, {4}}};

  q2 = q1;

  return q2.d.b0 == 3;
}

static_assert(f());

//cwg: 3070
//title: Trivial assignment can skip member subobjects
//meeting: Kona 11/25
//edg_status: EDGcpfe/28546
