//options_all:-r -x -tused
//options: --strict;cn:;rp

class A { int f(); friend int ff(); };
main() {
  class B { int g(); friend int gg(); };
}

