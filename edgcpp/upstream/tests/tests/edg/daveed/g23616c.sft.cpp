//remark:Qualified friend templ decl
//options:--c++14;fn:--c++14 --gnu=100100;fp:--c++14 --clang_v=70000;fp

namespace N {

template<typename... _Args>
void fn(_Args&&... __args);

}

struct CIArray {
template<class T> friend void N::fn(T&&);
template<class T> friend void N::fn(const T&);
};

void f() {
      CIArray l;
}
