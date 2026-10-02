//type: fp
//options: 
struct A {};

template <typename T> struct B
{
  B() { A().A::~A(); }
};

B<void> b;
