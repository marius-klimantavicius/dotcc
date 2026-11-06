import json,os,subprocess,sys
from pathlib import Path
E=Path(__file__).resolve().parent; R=E.parents[3]; W=Path('/mnt/storage/dotcc-libghostty-disposable/resume-20261104/color-scheme-declaration');W.mkdir(parents=True,exist_ok=True)
base=json.loads((E.parent/'direct-upstream/actual-function-native-name.json').read_text())['command'];root=Path(next(v.removeprefix('-Mroot=') for v in base if v.startswith('-Mroot=')));subject=root.with_name('color-scheme-declaration-proof.zig')
subject.write_text('''const std=@import("std"); const dev=@import("terminal/device_status.zig"); const opts=@import("terminal_options");
pub fn main() u8 {
 const Backing=@typeInfo(dev.ColorScheme).@"enum".tag_type;
 if(Backing != (if(opts.c_abi) c_int else u1)) return 1;
 if(@sizeOf(dev.ColorScheme)!=@sizeOf(Backing) or @alignOf(dev.ColorScheme)!=@alignOf(Backing)) return 2;
 if(@backingInt(dev.ColorScheme.light)!=0 or @backingInt(dev.ColorScheme.dark)!=1) return 3;
 if(@typeInfo(dev.ColorScheme).@"enum".field_names.len!=2) return 4;
 if(dev.max_color_scheme_report_encode_size!=9) return 5;
 for([_]dev.ColorScheme{.light,.dark}) |scheme| {
  for(0..17) |capacity| {
   var bytes:[19]u8=@splat(0xa5);var writer=std.Io.Writer.fixed(bytes[1..1+capacity]);
   if(dev.encodeColorSchemeReport(&writer,scheme)) |_| {if(capacity<9) return 6;} else |_| {if(capacity>=9) return 7;}
   const expected=if(scheme==.dark) "\\x1b[?997;1n" else "\\x1b[?997;2n";
   const written=writer.buffered();if(written.len!=@min(capacity,9) or !std.mem.eql(u8,written,expected[0..written.len]))return 8;
   if(bytes[0]!=0xa5 or bytes[1+capacity]!=0xa5) return 9;
  }
 }
 return 42;
}
''')
rows=[];variant=sys.argv[1];env=dict(os.environ,TMPDIR=str(W.parent/'tmp'))
oldopts=Path(next(v.removeprefix('-Mterminal_options=') for v in base if v.startswith('-Mterminal_options=')))
for c in [True,False]:
 options=W/('options-'+str(c)+'.zig');options.write_text(oldopts.read_text().replace('pub const c_abi: bool = true;', 'pub const c_abi: bool = '+str(c).lower()+';'))
 for mode in ['Debug','ReleaseFast']:
  output=W/(variant+'-'+str(c)+'-'+mode);cmd=[str(output)];args=[]
  for a in base:
   if a=='build-obj':args.extend(['build-exe','-O',mode])
   elif a=='-fno-emit-bin':args.append('-femit-bin='+str(output))
   elif a.startswith('-Mroot='):args.append('-Mroot='+str(subject))
   elif a.startswith('-Mterminal_options='):args.append('-Mterminal_options='+str(options))
   else:args.append(a)
  built=subprocess.run(args,env=env,capture_output=True,text=True);rows.append(dict(command=args,exit=built.returncode,stderr=built.stderr));assert built.returncode==0,built.stderr
  result=subprocess.run(cmd,env=env,capture_output=True,text=True);rows.append(dict(command=cmd,exit=result.returncode,stdout=result.stdout,stderr=result.stderr));assert result.returncode==42,(result.returncode,result.stderr)
(E/(variant+'-native.json')).write_text(json.dumps(rows,indent=2)+'\n');print(variant,'C/Zig × Debug/Fast layout/ordinal/34 bounded writer cases PASS42')
