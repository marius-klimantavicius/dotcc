// Native argv lives in the program's heap ledger until Shutdown, including
// global destructor execution. A managed host may explicitly supply argv[0].
bool main_arguments(a_routine_ptr r) {
  auto rt=skip_typerefs(r->type);
  if(type(rt->variant.routine.return_type)!="int")unsupported("main return ABI");
  auto p=scope_for_routine(r)->variant.routine.parameters;
  if(!p)return false;
  if(type(p->type)!="int" || !p->next || type(p->next->type)!="sbyte**" || p->next->next)
    unsupported("main arguments: expected int main(int,char**)");
  return true;
}
void emit_main(a_routine_ptr r) {
  const string mod=instance_mode?"":"static ";
  bool arguments=main_arguments(r);
  if(arguments) {
    line("// dot-gen:main-arguments utf8-owned-v1");
    line("// dot-gen:member-begin");
    line("public "+mod+"int Run() => Run(global::System.Environment.GetCommandLineArgs());");
    line("// dot-gen:member-end");
  }
  line("// dot-gen:member-begin");
  line("public "+mod+"int Run("+string(arguments?"string[] arguments":"")+") {");
  if(lifecycle_emitted)line("__edg_enter();");
  line("try {");
  if(arguments) {
    line("global::System.ArgumentNullException.ThrowIfNull(arguments);");
    line("ulong bytes=checked(((ulong)arguments.Length+1)*8);");
    line("foreach(string argument in arguments) { global::System.ArgumentNullException.ThrowIfNull(argument); if(argument.IndexOf('\\0')>=0)throw new global::System.ArgumentException(\"Program arguments cannot contain NUL\",nameof(arguments)); bytes=checked(bytes+(ulong)global::System.Text.Encoding.UTF8.GetByteCount(argument)+1); }");
    line("sbyte** argv=(sbyte**)__edg_heap.Allocate(bytes,8); if(argv==null)throw new global::System.OutOfMemoryException();");
    line("byte* text=(byte*)(argv+arguments.Length+1);");
    line("for(int i=0;i<arguments.Length;++i) { argv[i]=(sbyte*)text; string argument=arguments[i]; int count=global::System.Text.Encoding.UTF8.GetByteCount(argument); global::System.Text.Encoding.UTF8.GetBytes(global::System.MemoryExtensions.AsSpan(argument),new global::System.Span<byte>(text,count)); text[count]=0;text+=count+1; }");
    line("argv[arguments.Length]=null;");
  }
  line("return "+name(r,"f")+"("+string(arguments?"arguments.Length,argv":"")+"); }");
  line("finally { "+string(lifecycle_emitted?"__edg_leave(); ":"")+"global::System.GC.KeepAlive("+string(instance_mode?"this":"__edg_storage")+"); }");
  line("}\n// dot-gen:member-end");
}
