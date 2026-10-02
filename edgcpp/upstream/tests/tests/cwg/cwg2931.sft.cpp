//type:fn
//options_all:--c++23 
  struct B {
    bool operator==(this int, int);
    operator int() const;
  };

//cwg: 2931
//title: Restrictions on operator functions that are explicit object member functions
//meeting: Wroclaw 11/24
//edg_status: Passes
