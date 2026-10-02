//type: fp
//options: --c++11
// PR c++/54891
// { dg-do compile { target c++11 } }

int main()
{
  (void)[]{};
}
