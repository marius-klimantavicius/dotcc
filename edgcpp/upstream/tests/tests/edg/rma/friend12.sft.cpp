//options_all:-r -x -tused
//options: --strict;cn

class A;
class X { 
  friend class A { };
  friend class B { };
  friend class { };
};

