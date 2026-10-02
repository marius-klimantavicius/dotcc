//type:fp
//options_all:--c23
//remark:[6.5] C23: Allow [[maybe_unused]] on labels
// 2/6/23   [EDGcpfe/25872]
//
// C23: Allow [[maybe_unused]] on labels
//
// As described in WG14 paper N2662, the front end now accepts the maybe_unused
// attribute on labels (and suppresses a warning about unused labels when
// appropriate) in C23 mode.  Note that the C++ standard does not allow the
// maybe_unused attribute to appertain to labels though most implementations
// allow it.  An error is now given in strict C++ mode and silently accepted in
// other C++ modes.
void f() {
  [[maybe_unused]]  // Previously an error or warning depending on the mode.
    label:          // Previously a warning about an unused label.
    return;
}
