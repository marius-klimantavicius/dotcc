//type:fp
//options_all:--gnu=40700
//remark:[4.13] GNU compatibility: Accept __final as a synonym for "final"
// 10/13/16 [EDGcpfe/14942,EDGcpfe/17618]
//
// GNU compatibility: Accept __final as a synonym for "final"
//
// In g++ emulation mode when gnu_version >= 40700, the __final keyword is
// now accepted as a synonym for "final".
struct A __final {};
