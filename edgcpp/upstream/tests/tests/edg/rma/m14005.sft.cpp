//options_all:-r -x -tused
//options: --strict;cn

// #005 _141p16 (syntax)
  // template-argument: arg-declaration

template < template<class T> > class x { T i; }; // error - syntax

