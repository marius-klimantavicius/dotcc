//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

namespace N {
  template<class T> void swap(T&, T&) { }
  struct list {
    int data;
    void swap(list& that) { N::swap(this->data, that.data); }
  };
}

