//type:rp
//options::--gnu_version 80000:--clang
//options_all:--c++11 --no_exceptions
struct A {
  int x = 37;
  int f() {
    return [=]()->int{
      struct B {
	int y = 100;
	int z = [=]{return this->y + 10;}();
      };
      return B().z;
    }();
  }
  int w = x + f();
};

int main()
{
  if (A().w != 147)
    return 1;

  return 0;
}
