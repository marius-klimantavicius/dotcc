struct Entry {
  int* file;
  Entry(int* p):file(p){}
  inline int* get_file() const { return this->file; }
};
template<typename T>struct Buffer:Entry {
  T val;
  Buffer(int* p):Entry(p),val{}{}
  inline int* route()const{return get_file();}
};
struct Child:Buffer<int> {Child(int*p):Buffer(p){} int read()const{return *get_file();}};
