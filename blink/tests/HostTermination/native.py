import resource,subprocess,sys
resource.setrlimit(resource.RLIMIT_CORE,(0,0))
for indirect in range(2):
    for kind in range(4):
        result=subprocess.run([sys.argv[1],str(kind),str(indirect)],capture_output=True,timeout=5)
        assert result.returncode==(-6 if kind==3 else 37),(kind,indirect,result.returncode)
print('host termination: direct and C function-pointer calls preserve kind/status and never return: PASS')
