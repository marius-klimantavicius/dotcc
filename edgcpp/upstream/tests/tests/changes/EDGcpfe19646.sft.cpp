//type:fp
//remark:[5.0] Member function qualifiers and alias template declarations
// 5/2/18   [EDGcpfe/19646]
//
// Member function qualifiers and alias template declarations
//
// The front end previously issued a spurious error on an alias template
// declaration denoting a qualified member function type.
//
// That is now fixed.
template<class T> using X = T() const;  // Previously an error.  Now okay.
