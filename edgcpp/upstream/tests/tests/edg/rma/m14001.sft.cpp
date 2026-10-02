//options_all:-r -x -tused
//options: --strict;cn

// #001 _141p12 (syntax) 
  // template-declaration: template < template-argument-list > decl

template  class T> class x { T i; };  // error - syntax

