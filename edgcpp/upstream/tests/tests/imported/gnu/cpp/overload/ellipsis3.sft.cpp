//type: fn
//options: 
struct A {
  A(...);
};

int main()
{
  volatile A a;
  volatile A a2(a);		// { dg-error "volatile" }
}
