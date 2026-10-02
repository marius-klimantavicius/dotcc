//type: fp
//options: --c++11
// PR c++/28558
// { dg-options "" }
// { dg-do compile { target c++11 } }

struct A
{
  A(int) { }
};

int main()
{
    A a = (A [[gnu::unused]])0; // { dg-warning "attribute" }
}
