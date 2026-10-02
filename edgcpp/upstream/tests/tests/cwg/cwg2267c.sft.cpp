//type: fn
//options: 
//options_all: -A --c++20 -tused -e 200 --no_wrap

   struct D { D(); }; 
   struct C { explicit operator D(); } c; 

  const D &d2(c);  // error: cannot copy-initialize B temporary from A
