//type:fp
//options_all:--microsoft
//remark:[4.10] Microsoft compatibility: Truncation of enum constant values
// 7/14/14  [EDGcpfe/6235,EDGcpfe/9112,EDGcpfe/14852,EDGcpfe/15284]
//
// Microsoft compatibility: Truncation of enum constant values
//
// In Microsoft modes, the front end now allows the specified constant value of an
// enumerator to be outside the representable range of the enumerator's underlying
// type.
//
// A warning is issued in such cases (and the value is truncated).
enum { e = static_cast<unsigned long long>(-1) };
                                         // Now accepted in Microsoft mode.
