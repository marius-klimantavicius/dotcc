//type: fp
//options: --c++11
// PR c++/84091
// { dg-do compile { target c++11 } }

template < typename > void f ()
{ 
  [] { struct A {} a; } ();
}

int main ()
{ 
  f < int > ();
  return 0;
}
