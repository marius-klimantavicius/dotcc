using System.Text.Json;
using Managed.Networking;
using static Managed.Networking.Lwip;

unsafe class Program
{
    static readonly List<Lwip> DestroyOwners=[];
    static void Require(bool value,string name) { if(!value)throw new InvalidOperationException(name); }
    static void Destroy(Lwip owner,byte id,void* data)
    { Require(id==1 && data!=null,"extension callback identity");(*(uint*)data)++;DestroyOwners.Add(owner); }
    static ip_addr Address(Lwip owner,string text)
    { ip_addr result=default;fixed(byte* bytes=System.Text.Encoding.UTF8.GetBytes(text+"\0"))Require(owner.ipaddr_aton(bytes,&result)==1,"address parse");return result; }
    static ip6_addr Address6(Lwip owner,string text)
    { ip6_addr result=default;fixed(byte* bytes=System.Text.Encoding.UTF8.GetBytes(text+"\0"))Require(owner.ip6addr_aton(bytes,&result)==1,"address6 parse");return result; }
    static void Run(Lwip owner,JsonElement native)
    {
        using var binding=owner.__DotCcEnter();LwipSysArch.SetClock(owner,()=>1000);owner.lwip_init();owner.tcp_md5_init();
        byte id=owner.tcp_ext_arg_alloc_id();Require(id==1,"per-owner extension id allocation");
        uint destroyed=0;tcp_ext_arg_callbacks callbacks=new(){destroy=&Destroy};tcp_pcb* pcb=owner.tcp_new();Require(pcb!=null,"pcb allocation");
        owner.tcp_ext_arg_set_callbacks(pcb,id,&callbacks);owner.tcp_ext_arg_set(pcb,id,&destroyed);
        Require(owner.tcp_md5_get_additional_option_length(pcb,4)==4,"unkeyed MD5 option length");owner.tcp_abort(pcb);
        Require(destroyed==1 && ReferenceEquals(DestroyOwners[^1],owner),"extension destroy callback retained owner");
        netif iface=default;ip6_prefix broad=new(){addr=Address6(owner,"2001:db8::"),prefix_len=32},specific=broad,bad=broad;
        specific.prefix_len=64;bad.prefix_len=33;ip6_addr target=Address6(owner,"2001:db8::1234");sbyte idx=-1;
        Require(owner.ip6_static_route(null,&target)==null,"initial owner route table empty");
        Require(owner.ip6_add_route_entry(&broad,&iface,null,&idx)==0,"broad route");
        Require(owner.ip6_add_route_entry(&specific,&iface,&broad.addr,&idx)==0,"specific route");
        Require(owner.ip6_find_route_entry(&target)==0 && owner.ip6_static_route(null,&target)==&iface,"longest-prefix route");
        Require(owner.ip6_get_gateway(&iface,&target)==&broad.addr,"gateway pointer");
        Require(owner.ip6_add_route_entry(&bad,&iface,null,null)==(sbyte)err_enum_t.ERR_ARG,"non-byte prefix rejection");
        owner.ip6_remove_route_entry(&specific);Require(owner.ip6_find_route_entry(&target)==0,"fallback route");
        owner.ip6_remove_route_entry(&broad);Require(owner.ip6_find_route_entry(&target)==0 && owner.ip6_static_route(null,&target)==null,"native empty-slot index and no-route hook");
        byte* secret=stackalloc byte[16];for(int i=0;i<16;i++)secret[i]=(byte)(i*17);
        owner.lwip_init_tcp_isn(7,secret);ip_addr local=Address(owner,"192.0.2.1"),remote=Address(owner,"198.51.100.2");
        Require(owner.lwip_hook_tcp_isn(&local,1234,&remote,4321)==native.GetProperty("isn4").GetUInt32(),"native IPv4 ISN");
        local=Address(owner,"2001:db8::1");remote=Address(owner,"2001:db8::2");
        Require(owner.lwip_hook_tcp_isn(&local,1234,&remote,4321)==native.GetProperty("isn6").GetUInt32(),"native IPv6 ISN");
        LwipSysArch.Stop(owner);
    }
    static int Main(string[] args)
    {
        using var document=JsonDocument.Parse(File.ReadAllText(args[0]));var native=document.RootElement;var abi=native.GetProperty("abi");
        tcp_pcb pcb=default;
        var sizes=new Dictionary<string,long>{["netif"]=sizeof(netif),["tcp_pcb"]=sizeof(tcp_pcb),["tcp_ext_args_offset"]=(byte*)&pcb.ext_args-(byte*)&pcb,["tcp_ext_arg_callbacks"]=sizeof(tcp_ext_arg_callbacks),["ip6_prefix"]=sizeof(ip6_prefix),["ip6_route_entry"]=sizeof(ip6_route_entry),["tcp_md5sig"]=sizeof(tcp_md5sig)};
        foreach(var pair in sizes)Require(pair.Value==abi.GetProperty(pair.Key).GetInt64(),"native layout "+pair.Key);
        using var first=new Lwip();using var second=new Lwip();Run(first,native);Run(second,native);
        Require(DestroyOwners.Count==2,"one callback per owner");
        Console.WriteLine("PASS contrib-addons: seven native ABI checks; two-owner extension callbacks, route table operations and native IPv4/IPv6 ISNs");return 0;
    }
}
