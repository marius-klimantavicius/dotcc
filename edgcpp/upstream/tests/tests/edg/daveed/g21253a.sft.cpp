//remark:Dependent calls
//options:--c++17 -A --exceptions -tused;fp:--c++17;fp

auto cat() {
  return [](auto&& f) {
    return static_cast<decltype(f)&&>(f)();
  };
}

int foo(void);

int main()
{
  auto j = cat()(&foo);
  return 0;
}
