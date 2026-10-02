//options_all:--c++23 -tused -A
  struct D {
    bool operator==(this D const&, D const&) = default; 
  };
