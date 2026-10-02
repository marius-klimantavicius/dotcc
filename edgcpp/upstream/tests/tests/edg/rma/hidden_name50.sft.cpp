//options_all:-r -x -tused
//options: --strict;cp

  namespace N {
    template<class T> struct iterator { };
    struct vector_bool {
      struct iterator;
      struct iterator : N::iterator<vector_bool> { };
    };
  }


