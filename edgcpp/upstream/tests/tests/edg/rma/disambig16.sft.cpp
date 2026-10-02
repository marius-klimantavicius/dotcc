//options_all:-r -x -tused
//options: --strict;ln

typedef int I;
int i;
struct S {
  typedef int I;
  static int i;
  S(int);
  S();
};
int main() {
  S a(::I);        // function declaration
  S b(::i);        // variable declaration with initialization
  S c(S::I);       // function declaration
  S d(S::i);       // variable declaration with initialization
}

