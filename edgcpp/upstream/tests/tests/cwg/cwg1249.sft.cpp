//type:fn
//options_all:--c++20 -tused -A

void f(int i) {
      auto l1 = [i] {
        auto l2 = [&i] {
          ++i;    
        };
      };
    }

//cwg: 1249
//title: Cv-qualification of nested lambda capture
//meeting: Virtual 10/21
//edg_status: Passes
