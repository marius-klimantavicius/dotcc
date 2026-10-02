//remark:Explicit default constructors
//options:--c++14;fn:--microsoft_v=1924;fp

struct S { explicit S(int = 0) {} };
struct X { S s; };
X x = {};  // Normally an error, but okay in Microsoft mode.


