//options_all:-r -x -tused
//options: --strict;cp

struct S {
  operator S ();
  operator S& ();
  operator const S ();
  operator const S& ();
};

