//options_all:-r -x -tused
//options: --strict;cn:;cp

class A;
typedef A TA;
typedef const TA CTA;
class B {
  friend TA;
  friend CTA;
};

