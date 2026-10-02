//type:fp
//options_all:--microsoft_version 1913 --c++
//remark:[5.0] Microsoft compatibility: allow __inline to be used with namespace
// 4/23/18  [EDGcpfe/19564]
//
// Microsoft compatibility: allow __inline to be used with namespace
//
// In Microsoft mode, __inline could not be used to declare an inline
// namespace. This has now been fixed.
// --microsoft_version 1913 --c++):
__inline namespace literals {} //declares an inline namespace
