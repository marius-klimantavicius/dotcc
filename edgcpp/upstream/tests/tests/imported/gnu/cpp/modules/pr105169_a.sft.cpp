//type: fp
//options:  --c++11 --c++11
# 0 "./modules/pr105169_a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/pr105169_a.C"







# 1 "./modules/pr105169.h" 1
class IPXAddressClass
{
public:
    IPXAddressClass(void);
};

class WinsockInterfaceClass
{

public:
    WinsockInterfaceClass(void);

    virtual void Set_Broadcast_Address(void*){};

    virtual int Get_Protocol(void)
    {
        return 0;
    };

protected:
};
# 9 "./modules/pr105169_a.C" 2

WinsockInterfaceClass* PacketTransport;

IPXAddressClass::IPXAddressClass(void)
{
}

int function()
{
  return PacketTransport->Get_Protocol();
}

int main()
{
  IPXAddressClass ipxaddr;
  return 0;
}
