template<class T> class A
{
  T mem1;
  T mem2;

public:
  A(T v1,T v2): mem1(v1),mem2{v2} {}
  void set_mem1(T v) { mem1 = v;}
  void set_mem2(T v) { mem2 = v;}

  T get_mem1(void) { return mem1; }
  T get_mem2(void) { return mem2; }
} ;

int main() {
    // Creating int
    auto a_int = new A{1,2};
    auto res_int = a_int->get_mem1() + a_int->get_mem2();
    a_int->set_mem1(42);
    a_int->set_mem2(42);
    res_int = a_int->get_mem1() - a_int->get_mem2();

    return 0;
}
