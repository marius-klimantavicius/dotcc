//type:fp
//options_all:--microsoft
//remark:[4.1] Microsoft compatibility: #pragma conform(forScope, ...)
// 4/29/09  [EDGcpfe/9459]
//
// Microsoft compatibility: #pragma conform(forScope, ...)
//
// In Microsoft modes, the front end now accepts the following pragma syntax to
// control the behavior of declarations in for-init-scope:
//
// ([...] indicates an optional component; | indicates a choice between tokens).
//
// The first form with "on" enables standard for-init-scope behavior.  The "off"
// variant enables the for-init-scope behavior of the current Microsoft mode,
// except when microsoft_version >= 1310: In that case the default behavior when
// microsoft_version == 1310 is enabled.
//
// The second form ("show") triggers a warning that reflects the current state
// of the pragma.
//
// The third form with "push" pushes the state of the current behavior on a stack
// and, optionally, associates a given identifier with that state.  The "pop"
// variant without an identifier restores the last recorded state and pops it
// from the stack.  The variant with an identifier restores the last state that
// was pushed with the same identifier and pops it and all later recorded states
// from the stack.  The "push" and "pop" variants that included an identifier
// naming a state can also be followed by "on" or "off", which behaves as if the
// first form above had followed the "push" or "pop".
//
// In C mode, the pragma is recognized and parsed, but it has no effect.
#pragma conform(forScope, push, xxx, on)
  // Standard for-scope rules are now in effect.  A state describing the
  // default rules is on the stack.
void f1() {
  for (int i; false;);
  for (int i; false;);  // Okay
}
#pragma conform(forScope, off)
  // Old-style rules are in effect (the default state is still on the
  // stack).
int f2() {
  for (int i = 2; false;);
  return i;  // Okay.
}
#pragma conform(forScope, pop, xxx)
  // Default for-scope rules are restored.
#pragma conform(forScope, show)
  // Triggers a warning indicating whether the current for-scope rules
  // conform to the standard or not.
