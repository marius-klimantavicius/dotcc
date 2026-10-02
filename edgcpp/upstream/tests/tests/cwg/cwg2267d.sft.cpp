//type: fp
//options: 
//options_all: -A --c++20 -tused -e 200 --no_wrap
   struct D { D(); }; 
   struct C { explicit operator D(); } c; 
   D d1(c);            // ok 
