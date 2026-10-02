//type:fp
//remark:[4.11] __is_convertible_to and class types
// 1/6/16   [EDGcpfe/16645]
//
// __is_convertible_to and class types
//
// The front end's implementation of __is_convertible_to has been reworked to more
// accurately test the criteria specified for the standard C++ library trait
// std::is_convertible.  In particular, this corrects cases involving class types
// with inaccessible constructors.
struct N {
private:
  N(N&);
};
static_assert(!__is_convertible_to(N&, N), "?");
    // Previously failed; now okay.
