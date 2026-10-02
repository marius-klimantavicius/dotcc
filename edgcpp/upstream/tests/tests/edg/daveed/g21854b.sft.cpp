//remark:__final in templates
//options:--c++11;fn:--c++11 --gnu=40800;fp:--c++11 --clang;fp

  template<typename> struct X __final {};  // Previously always an error.
                                           // Now accepted in GNU C++11 mode.

