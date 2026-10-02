//type: fp
//options: 
# 0 "./template/access37a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./template/access37a.C"





# 1 "./template/access37.C" 1


template <class T>
struct EnumeratorRange {
  struct Iterator {
    EnumeratorRange range_;

    friend void f(Iterator i) {
      i.range_.end_reached_;
      i.range_.EnumeratorRange::end_reached_;
      &i.range_.end_reached_;
      &i.range_.EnumeratorRange::end_reached_;
    }
  };

 private:
  bool end_reached_;



};

int main() {
  EnumeratorRange<int>::Iterator i;
  f(i);
}
# 7 "./template/access37a.C" 2
