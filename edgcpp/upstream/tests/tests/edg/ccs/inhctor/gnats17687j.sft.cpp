//type:rp
//options_all:--c++17

extern "C" int printf(const char *, ...);

class A
{
  virtual void foo() {}
};

class B : public A
{
public:
 B (int b = 0) : bi(10) {}
 using A::A;
 int bi = 5;
};

B *b = new B();

int main() {
  printf("%d\n", b->bi);
  
  if (b->bi != 10) return 1;
}
