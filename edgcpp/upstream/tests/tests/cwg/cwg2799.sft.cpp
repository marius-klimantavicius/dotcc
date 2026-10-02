//type:fp
//options: -A --c++20

struct A { int n; };

struct B : A {
  using A::A;
  B(int);
};

B b;

//cwg: 2799
//title: Inheriting default constructors
//meeting: Croydon 3/26
//edg_status: Passes
