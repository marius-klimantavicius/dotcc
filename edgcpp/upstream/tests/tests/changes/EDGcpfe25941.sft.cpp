//type:fn
//options_all:--strict --c23
//remark:[6.5] C23: Trigraphs disabled
// 1/16/23  [EDGcpfe/25941]
//
// C23: Trigraphs disabled
//
// As described in WG14 paper N2940, the front end no longer accepts trigraphs
// in C23 mode.  Trigraphs can continue to be used via the --trigraphs
// command-line option.
// --strict --c23:
int i = ??-0;   // Now an error in C23 mode
