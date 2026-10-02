//options_all:-r -x -tused
//options: --strict;cp

class A {
  private:
    A();
    static A a;
};

A A::a;

int main () { return 0; }


