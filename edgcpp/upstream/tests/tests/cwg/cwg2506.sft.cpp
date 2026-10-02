//type:fp
//options_all:--c++20 -tused -A
       const int arr[1]{};
       void f() {
          auto [i] = arr;
         i = 5;  // Now well-formed, previously an error
       }
