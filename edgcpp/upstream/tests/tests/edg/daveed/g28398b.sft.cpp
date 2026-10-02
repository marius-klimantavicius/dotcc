//remark:Rendering of indeterminate template arguments
//options:--c++20;fn

  template<typename> concept C = false;
  template<C T> using A = void;
  struct S {};
  template<typename> struct X;
  template<typename T, typename = A<T>, template<typename> class = X> void g() {}
  int main() {
    g<S>();  // Previously triggered an abort.  Now an ordinary error message.
  }

