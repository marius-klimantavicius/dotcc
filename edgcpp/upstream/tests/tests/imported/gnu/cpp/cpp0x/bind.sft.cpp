//type: fp
//options: --c++11
// { dg-do compile { target c++11 } }
struct S{};
void f(S&&);

int main()
{
  f(S());
}
