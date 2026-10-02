//type:fn
//options_all:--c++20 -tused 
export module M;
namespace {
  export int a2;                // error: export of name with internal linkage
}
