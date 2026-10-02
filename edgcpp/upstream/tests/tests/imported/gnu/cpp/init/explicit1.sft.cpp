//type: fp
//options:  --c++11
// PR c++/60417
// { dg-options -pedantic }

struct A { explicit A(int = 0); };
struct B { A a; };

int main()
{
  B b = {};			// { dg-warning "explicit" "" { target c++11 } }
}
