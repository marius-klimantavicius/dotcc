import ctypes,sys
lib=ctypes.CDLL(sys.argv[1])
assert lib.Errors()==0
assert lib.Exchange(0)==0
print('private TCP messages: peers, gather/scatter, short transfers, EOF: PASS')
