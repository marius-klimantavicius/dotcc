//type:fp
//options_all:--c --gcc
//remark:[4.4] GNU C compatibility: Excess aggregate initializers
// 12/6/11  [EDGcpfe/12439]
//
// GNU C compatibility: Excess aggregate initializers
//
// In GNU modes, the front end ignores excess aggregate initializers (with a
// warning) in various situations.  However, previously, excess initializers were
// not permitted if they appeared inside the superfluous braces surrounding an
// initializer for a simple non-aggregate value.
//
// Now the excess initializers in such contexts (the "2.0" in this example) are
// also ignored with a warning in GNU C mode.  (See also the Changes entry of
double z[1] = { { 1.0, 2.0 } };  // Previously an error in all modes.
                                 // Now accepted in GNU C mode.
