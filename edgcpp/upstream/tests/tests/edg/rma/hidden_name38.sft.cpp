//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn:;cp

// EDGqa01372
namespace std {
  template<class P> inline void swap(P&, P&) { }
 
  struct list {
    int data;
    void swap(list& that) { std::swap(this->data, that.data); }
    friend void swap(list&, list&) { }
  };

  struct allocator { typedef int size_type; };
 
  template<> inline void swap(allocator&, allocator&) { }
 
  template<class A> struct Tlist {
    typedef A::size_type my_size_type;
    bool data1;
    void* data2;
    my_size_type data3;
    void swap(Tlist& that) {
      std::swap(this->data1, that.data1);
      if (1) {
        std::swap(this->data2, that.data2);
        std::swap(this->data3, that.data3);
      }
    }
  };

} /* end of namespace std */

using namespace std;
typedef Tlist<allocator> TL;
 
void foo() {
  TL tl1, tl2;
  tl1.swap(tl2);
}


