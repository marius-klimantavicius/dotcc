//type:fn
//options_all:--c++20 -tused -A
     enum class E { zero };
     enum class I : int { };
     I i{E::zero};  // Previously unintentionally allowed, now ill-formed

//cwg: 2374
//title: Overly permissive specification of enum direct-list-initialization
//meeting: Belfast 11/19
//edg_status: Passes
