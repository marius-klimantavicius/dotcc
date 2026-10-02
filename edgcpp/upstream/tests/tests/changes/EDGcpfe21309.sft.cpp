//type:fp
//options_all:--ms_permissive --microsoft --no_ms_permissive
//remark:[6.2] Microsoft compatibility: forward declaration of enum type
// 1/28/21  [EDGcpfe/21309]
//
// Microsoft compatibility: forward declaration of enum type
//
// The C++ standard prohibits a forward declaration of an enum type but
// Microsoft allows it.  The front end had also allowed in Microsoft emulation
// mode, but only with --ms_permissive.  A change has been made to accept this
// in all Microsoft emulation modes.
// --no_ms_permissive:
enum E; // Now accepted even with --no_ms_permissive
