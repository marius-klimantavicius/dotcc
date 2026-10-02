//type:fp
//options:--c++26 --set_flag reflection
//options_all:-A -tused

namespace std
{
  namespace meta
  {
    using info = decltype(^^::);
  }
}

struct B
{
  virtual void f(std::meta::info)
  { }
};

struct D : B
{
  consteval void f(std::meta::info) override
  { }
};

//cwg: 3117
//title: Overriding by a consteval virtual function
//meeting: Kona 11/25
//edg_status: EDGcpfe/28550
