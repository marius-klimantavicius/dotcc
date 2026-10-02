//type:fn
//options_all:--c++23 -tused
 template<typename T>
  concept C = requires(T t, ...) {  // error: terminates with an ellipsis
    t;
  };

//cwg: 2560
//title: Parameter type determination in a requirement-parameter-list
//meeting: Tokyo 3/24
//edg_status: Passes
