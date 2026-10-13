/* Proposed Embedded C projection, --named_registers; uncompiled by root.
   targ_def.h5687-5708 stock test register _EDG_REG_2 has long-sized storage. */
register _EDG_REG_2 long variable_named_register;
long variable_use_named_register(long value) {
  variable_named_register = value;
  return variable_named_register;
}
