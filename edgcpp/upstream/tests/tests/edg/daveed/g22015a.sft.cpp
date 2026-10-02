//remark:Template nontype arguments
//options:--gnu=50400 --c++11;fn


  template<char(*)()> struct X {};
  extern int g();
  int main() {
    X<(char (*)())g> x;
  }
