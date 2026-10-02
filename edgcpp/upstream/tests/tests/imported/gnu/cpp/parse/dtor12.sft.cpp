//type: fn
//options:  --c++20
// PR c++/29408

template <class T> class a
{
  ~a<T>(); // { dg-error "template-id not allowed for destructor" "" { target c++20 } }
};
