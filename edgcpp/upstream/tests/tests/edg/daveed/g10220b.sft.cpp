//remark: Hiding of using-declarations
//options:-A;fp:;fp:--g++;fp:--microsoft;fp

class A
{
public:
  template<typename X>
  void f(const X&);
};
class B : public A
{
public:
  using A::f;
  template<typename X>
  void f(const X&);
};
class C
{

};
int main()
{
  B x; C y;
  x.f(y);
}
