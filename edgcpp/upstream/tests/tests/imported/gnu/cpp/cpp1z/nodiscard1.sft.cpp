//type: fp
//options: --c++11
// { dg-do compile { target c++11 } }

[[nodiscard]] int f();

int main()
{
  f();				// { dg-warning "" }
}
