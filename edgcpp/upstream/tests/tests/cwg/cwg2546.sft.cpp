//options_all:--c++23 -A
  struct HasNoLessThan { };
  struct C {
    friend HasNoLessThan operator<=>(const C&, const C&);
    bool operator<(const C&) const = default;  // OK, function is deleted
  };

//cwg: 2546
//title: Defaulted secondary comparison operators defined as deleted
//meeting: Tokyo 3/24
//edg_status: Passes
