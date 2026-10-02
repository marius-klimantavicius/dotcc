//type:fp
//options:--microsoft --microsoft_version=1928 -c:--microsoft --microsoft_version=1927 -c;fn:--g++;fn
//options_all:-W

struct C {
  [[msvc::noop_dtor]] ~C() {;}
};
