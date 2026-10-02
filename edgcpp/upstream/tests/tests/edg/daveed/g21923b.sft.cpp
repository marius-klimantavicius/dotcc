//remark:Extraneous namespace qualifier on templates
//options:--microsoft_v=1910;fp:--microsoft_v=1914;fn

  namespace N {
    template<typename> void N::f();  // Now accepted in some Microsoft modes.
  }
