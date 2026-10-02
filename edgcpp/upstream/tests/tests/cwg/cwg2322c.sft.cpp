  //type:fp
  //options_all:--c++20 -tused -A 
  template <class T> void h(...) { }

  void x() {
   h<int>(0);    // ill-formed, no diagnostic required
  }
