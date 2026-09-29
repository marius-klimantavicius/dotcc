#!/usr/bin/env python3
"""Provision only inside an unshared user+network namespace; no host changes."""
import argparse,json,os,pathlib,subprocess,sys
p=argparse.ArgumentParser();p.add_argument('--evidence',required=True);p.add_argument('command',nargs=argparse.REMAINDER);a=p.parse_args()
if os.environ.get('LWIP_PRIVATE_NETNS')!='1':
    os.execvpe('unshare',['unshare','--user','--map-root-user','--net',sys.executable,__file__,'--evidence',a.evidence,*a.command],{**os.environ,'LWIP_PRIVATE_NETNS':'1','LWIP_PARENT_NETNS':os.readlink('/proc/self/ns/net')})
if os.readlink('/proc/self/ns/net')==os.environ.get('LWIP_PARENT_NETNS'):raise RuntimeError('Network namespace did not change')
commands=[['ip','link','set','lo','up'],['ip','tuntap','add','dev','lwiptest0','mode','tap','user','0'],['ip','address','add','10.9.0.1/24','dev','lwiptest0'],['ip','-6','address','add','fd09::1/64','dev','lwiptest0','nodad'],['ip','link','set','lwiptest0','up']]
for command in commands:subprocess.run(command,check=True,capture_output=True,text=True)
evidence={'isolation':'new user+network namespace; no host route/interface changes','namespace':os.readlink('/proc/self/ns/net'),'parent_namespace':os.environ['LWIP_PARENT_NETNS'],'kernel':subprocess.check_output(['uname','-a'],text=True).strip(),'iproute2':subprocess.check_output(['ip','-Version'],text=True).strip(),'commands':commands,'interfaces':json.loads(subprocess.check_output(['ip','-j','address','show'],text=True)),'offload':'TUNSETOFFLOAD(0) asserted by test TAP attachment','product_dependency':'TAP ioctl/poll are test-only; published translated library has no TAP adapter'}
path=pathlib.Path(a.evidence);path.parent.mkdir(parents=True,exist_ok=True);path.write_text(json.dumps(evidence,indent=2)+'\n')
command=a.command[1:] if a.command and a.command[0]=='--' else a.command
if not command:raise RuntimeError('Missing test executable')
raise SystemExit(subprocess.run(command,timeout=120).returncode)
