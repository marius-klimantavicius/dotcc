//type:fp
//options_all:--microsoft
//remark:[4.2] Microsoft compatibility: __uuidof and explicit class template specialization
// 6/9/10   [EDGcpfe/10557]
//
// Microsoft compatibility: __uuidof and explicit class template specialization
//
// In Microsoft mode, the front end previously ignored a uuid specified on an
// explicit specialization of a class template when scanning a uuidof operator.
// This is now fixed.
template<typename T> class C;
template<>
  class __declspec(uuid("73BD59D0-7FB0-45E4-A44E-A4494EABE793")) C<int> {
  };
int main() {
  (void)__uuidof(C<int>);  // Previously an error.  Now accepted.
}
