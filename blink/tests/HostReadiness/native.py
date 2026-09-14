import ctypes,socket,sys,threading
lib=ctypes.CDLL(sys.argv[1])
assert lib.SocketErrors()==0
listener=lib.Prepare(0)
assert listener>=0
assert lib.ReadinessChecks(listener)==0
port=lib.Port(listener)
assert 0<port<65536
result=[]
worker=threading.Thread(target=lambda:result.append(lib.Serve(listener)))
worker.start()
with socket.create_connection(('127.0.0.1',port),timeout=5) as client:
    client.sendall(b'he');client.sendall(b'llo!')
    data=b''
    while len(data)<131072:
        chunk=client.recv(2048)
        assert chunk
        data+=chunk
    assert client.recv(1)==b''
assert data==bytes((i*13+7)&255 for i in range(4096))*32
worker.join(5)
assert not worker.is_alive() and result==[0]
listener=lib.Prepare(0)
port=lib.Port(listener)
result=[]
worker=threading.Thread(target=lambda:result.append(lib.Hangup(listener)))
worker.start()
with socket.create_connection(('127.0.0.1',port),timeout=5) as client:
    client.shutdown(socket.SHUT_WR)
    assert client.recv(1)==b''
worker.join(5)
assert not worker.is_alive() and result==[0],result
print('translated C poll: IPv4 bytes, fragmented request, exact 128 KiB response, EOF: PASS')
