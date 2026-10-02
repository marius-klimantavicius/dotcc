//options_all:--c++23 -tused -A
  template<class T> class stream;
  template<> class stream<char> { /* ... */ }; // #1

  template<class T> class Array { /* ... */ };
  template<class T> void sort(Array<T>& v) { /* ... */ }
   
  
  template<> void sort<int>(Array<int>&);   // #2
  template<> void sort<char*>(Array<char*>&);   // #3 template argument is deduced

//cwg: 2848
//title: Omitting an empty template argument list for explicit instantiation
//meeting: Tokyo 3/24
//edg_status: Passes
