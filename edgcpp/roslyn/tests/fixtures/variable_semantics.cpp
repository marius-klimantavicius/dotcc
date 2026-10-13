int variable_probe_init(int);
extern int variable_assembly_name asm("roslyn_assembly_variable");
thread_local int variable_dynamic_tls = variable_probe_init(3);
struct VariablePriority {
  VariablePriority(int);
  ~VariablePriority();
};
VariablePriority variable_priority __attribute__((init_priority(200))) (2);
int variable_register_and_tls(long input) {
  register long assigned asm("rax") = input;
  asm volatile("" : "+r"(assigned));
  return int(assigned) + variable_dynamic_tls + variable_assembly_name;
}
struct VariablePointerTypeInfo { virtual ~VariablePointerTypeInfo(); };
void variable_throw_pointer(VariablePointerTypeInfo *value) { throw value; }
