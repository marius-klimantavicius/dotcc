import subprocess,sys,threading
def child():
    process=subprocess.Popen([sys.executable,"-c","import time; memory=bytearray(96*1024*1024); time.sleep(10)"])
    process.wait()
worker=threading.Thread(target=child)
worker.start()
worker.join()
