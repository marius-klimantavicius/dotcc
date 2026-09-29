using System.Text.Json;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe class Program
{
    static void Main(string[] args)
    {
        using var document=JsonDocument.Parse(File.ReadAllText(args[0]));var native=document.RootElement.GetProperty("cases")[0].GetProperty("abi");
        pbuf buffer=default;
        var actual=new Dictionary<string,long>{["pbuf"]=sizeof(pbuf),["pbuf_payload"]=(byte*)&buffer.payload-(byte*)&buffer,["netif"]=sizeof(netif),["tcp_pcb"]=sizeof(tcp_pcb),["ip_addr"]=sizeof(ip_addr),["eth_hdr"]=sizeof(eth_hdr),["ip_hdr"]=sizeof(ip_hdr),["ip6_hdr"]=sizeof(ip6_hdr),["tcp_hdr"]=sizeof(tcp_hdr),["udp_hdr"]=sizeof(udp_hdr)};
        #if !LWIP_NOSYS
        netbuf nb=default;sockaddr_in ip4=default;sockaddr_in6 ip6=default;
        actual["sockaddr_in"]=sizeof(sockaddr_in);actual["sockaddr_in6"]=sizeof(sockaddr_in6);actual["sockaddr_storage"]=sizeof(sockaddr_storage);actual["fd_set"]=sizeof(fd_set);actual["netbuf"]=sizeof(netbuf);
        actual["netbuf_addr"]=(byte*)&nb.addr-(byte*)&nb;actual["netbuf_port"]=(byte*)&nb.port-(byte*)&nb;actual["sockaddr_in_addr"]=(byte*)&ip4.sin_addr-(byte*)&ip4;actual["sockaddr_in6_addr"]=(byte*)&ip6.sin6_addr-(byte*)&ip6;
#endif
        foreach(var pair in actual){long expected=native.GetProperty(pair.Key).GetInt64();if(pair.Value!=expected)throw new InvalidOperationException($"{pair.Key}: emitted {pair.Value}, native {expected}");Console.WriteLine($"PASS ABI {pair.Key}={pair.Value}");}
        using var owner=new Lwip();using(var binding=owner.__DotCcEnter())
        {
            var expected=document.RootElement.GetProperty("cases")[1].GetProperty("checksums");byte* bytes=stackalloc byte[18];for(int i=0;i<18;i++)bytes[i]=(byte)(i*17+3);
            pbuf tail=new(){payload=bytes+8,len=8,tot_len=8};pbuf head=new(){next=&tail,payload=bytes+1,len=7,tot_len=15};
            if(owner.inet_chksum(bytes+1,15)!=expected.GetProperty("odd_address_odd_length").GetUInt16()||owner.inet_chksum_pbuf(&head)!=expected.GetProperty("odd_chain").GetUInt16())throw new InvalidOperationException("Actual translated odd-address/odd-chain checksum differs from native");
            Console.WriteLine("PASS checksum: odd-address/odd-length and split pbuf chain match native");
        }
        Console.WriteLine("PASS abi: actual emitted layouts match independently compiled native structures");
    }
}
