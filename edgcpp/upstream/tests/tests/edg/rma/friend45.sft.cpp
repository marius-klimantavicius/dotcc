//options_all:-r -x -tused
//options: --strict;cn:;cp

class X { } x;
typedef X tX;
class Y {
  friend tX;
} y;
class Z {
  friend X;
} z;

