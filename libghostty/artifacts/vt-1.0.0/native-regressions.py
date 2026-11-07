"""Pinned native controls for the compiler fixes reached by vt-1.0.0."""
from pathlib import Path
import subprocess,os,json,shutil
R=Path(__file__).resolve().parents[3]
W=Path('/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/enum-native');W.mkdir(exist_ok=True)
S=R/'DotCC.FunctionalTests/Fixtures/ZigEnumTypeReceivers'
Z=W.parent/'tools/zig-x86_64-linux-0.17.0/zig'
for fixture in S.glob('*.zig'): shutil.copy2(fixture,W/fixture.name)
results=[]
for name in ['positive.zig','nonexhaustive.zig','comptime-enum-copy.zig','dependent-field-name.zig','comptime-type-comparison.zig','storage-reduce-or.zig','wide-constant-peer.zig','comptime-module-arrays.zig','comptime-mode-operand.zig','empty-packed-pointer.zig']:
 p=W/name;p.write_text((S/name).read_text()+'\npub fn main() u8 { return @intCast(answer()); }\n')
 for mode in (['ReleaseFast'] if name == 'comptime-mode-operand.zig' else ['Debug','ReleaseFast']):
  command=[str(Z),'build-exe',str(p),'-O'+mode,'--cache-dir',str(W/'cache'),'--global-cache-dir',str(W/'global-cache'),'-femit-bin='+str(W/'control')]
  c=subprocess.run(command,capture_output=True,text=True,env=dict(os.environ,TMPDIR=str(W)));(W/(name+'-'+mode+'.txt')).write_text(c.stdout+c.stderr);assert c.returncode==0,c.stderr
  c=subprocess.run([W/'control']);assert c.returncode==42,c.returncode
  results.append({'fixture':name,'mode':mode,'command':command,'exit':42})
shutil.copy2(S/'missing.zig',W/'missing.zig')
c=subprocess.run([Z,'build-obj',W/'missing.zig','--cache-dir',W/'cache','--global-cache-dir',W/'global-cache','-fno-emit-bin'],capture_output=True,text=True,env=dict(os.environ,TMPDIR=str(W)));assert c.returncode!=0;results.append({'fixture':'missing.zig','rejected':True})
Path(__file__).with_name('native-regressions.json').write_text(json.dumps(results,indent=2)+'\n');print(f'{sum(r.get("exit") == 42 for r in results)} native executions42 and missing enum rejection PASS')
