//type:fn
//options_all:--c++20 -W
struct dest_ty {};

using dest_typedef = dest_ty**;

struct src_ty {
  template<int>
  operator dest_typedef() {
    return nullptr;
  }
};
