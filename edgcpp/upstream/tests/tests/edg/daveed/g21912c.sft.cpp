//remark:Comparison rewrites
//options:--c++20;fp

             struct X {
               bool operator!=(const X&);
               bool operator==(const X&);
             };
             bool b = X() != X();
