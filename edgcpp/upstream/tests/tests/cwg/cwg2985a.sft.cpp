//type:fn
//options:--c++23 -A
struct X { };

struct Y : X {};

struct Z {
  operator const Y () const;
};

Z z;
X&& r = z; // #1, ill-formed; was well-formed before CWG1604

//cwg: 2985
//title: Unclear rules for reference initialization with conversion
//meeting: Sofia 6/25
//edg_status: Passes
