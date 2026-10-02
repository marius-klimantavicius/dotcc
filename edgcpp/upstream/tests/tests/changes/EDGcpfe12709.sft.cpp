//type:fp
//remark:[4.5] Spurious error on use of template with __underlying_type construct
// 2/21/12  [EDGcpfe/12709]
//
// Spurious error on use of template with __underlying_type construct
//
// When a template signature relied on the use of a typedef based on
// __underlying_type (e.g., in a return type), the front end sometimes spuriously
// failed argument deduction of that template.
//
// This is now fixed.  (The entry of 7/5/11 describes the introduction of the
// C++11 type traits helper __underlying_type, among other such helpers.)
template<typename T> struct U { typedef __underlying_type(T) R; };
template<typename T> typename U<T>::R f(T);
enum E { e };
auto x = f(e);  // Previously failed deduction, causing no matching f
                // to be found (i.e., an error).
