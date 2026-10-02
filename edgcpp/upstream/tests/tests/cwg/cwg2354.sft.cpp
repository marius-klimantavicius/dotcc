//type: fp
//options_all: -A --c++20 -tused -e 200 --no_wrap
  enum struct alignas(64) A {};
  A a[10]; 

//cwg: 2354
//title: Extended alignment and object representation
//meeting: Kona 02/19
//edg_status: Passes
