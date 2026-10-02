//type:fp
//remark:[4.12] Core issue 941: Specialization of deleted function templates
// 6/2/16   [EDGcpfe/10970,EDGcpfe/15318,EDGcpfe/17032]
//
// Core issue 941: Specialization of deleted function templates
//
// The front end now permits the explicit specialization of deleted function
// templates and deleted member functions of class templates.  This reflects the
// resolution of Core issue 941.
template<typename T> T f(T) = delete;
template<> int f(int p) { return p; }  // Now accepted.
int main() {
  f(42);  // Valid, since the specialization f<int>(int) is not deleted.
}
