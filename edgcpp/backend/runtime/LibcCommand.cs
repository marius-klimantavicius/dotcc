#nullable enable
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Edg.Runtime;

/// <summary>C17 host-command profile. Return values are BCL exit codes, not
/// POSIX wait statuses; source FILE replacement is not OS descriptor mutation.</summary>
public sealed unsafe class LibcCommand : IDisposable
{
    private static readonly Encoding utf8=new UTF8Encoding(false,true);
    private readonly Action<int> setErrno;
    private readonly string workingDirectory,processor;
    private readonly bool windows,supported;
    private bool disposed;

    public LibcCommand(Action<int> setErrno,string? workingDirectory=null,string? processorPath=null)
    {
        this.setErrno=setErrno??throw new ArgumentNullException(nameof(setErrno));
        this.workingDirectory=Path.GetFullPath(workingDirectory??Environment.CurrentDirectory);
        windows=OperatingSystem.IsWindows();
        supported=windows||OperatingSystem.IsLinux()||OperatingSystem.IsMacOS();
        processor=processorPath??(windows?Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"cmd.exe"):"/bin/sh");
    }

    private static string Decode(sbyte* value)
    {
        int length=0;while(value[length]!=0)length=checked(length+1);
        return utf8.GetString(new ReadOnlySpan<byte>(value,length));
    }

    private static int NativeError(int value)
    {
        if(OperatingSystem.IsWindows())return value switch {
            2 or 3 or 126=>2,5 or 32 or 33 or 740=>13,8 or 14=>12,
            193 or 216=>8,206=>36,267=>20,_=>5};
        if(OperatingSystem.IsMacOS())value=value switch{35=>11,63=>36,78=>38,_=>value};
        return value switch{1 or 2 or 5 or 7 or 8 or 11 or 12 or 13 or 20 or 22 or 24 or 26 or 36 or 38=>value,_=>5};
    }

    public int System(sbyte* command,sbyte** environment)
    {
        bool query=command==null;
        if(Volatile.Read(ref disposed)){setErrno(9);return query?0:-1;}
        if(!supported){setErrno(38);return query?0:-1;}
        try
        {
            string text=query?(windows?"exit /b 0":"exit 0"):Decode(command);
            var info=new ProcessStartInfo {FileName=processor,UseShellExecute=false,WorkingDirectory=workingDirectory};
            if(windows)
            {
                // cmd /s strips only the outer first/last quotes. Do not apply
                // the ordinary Windows argv quoting rules to its shell script.
                info.Arguments="/d /s /c \""+text+"\"";
            }
            else {info.ArgumentList.Add("-c");info.ArgumentList.Add(text);}
            info.Environment.Clear();
            if(environment!=null)
            {
                for(sbyte** item=environment;*item!=null;++item)
                {
                    string entry=Decode(*item);int equals=entry.IndexOf('=');
                    if(equals<=0){setErrno(22);return query?0:-1;}
                    info.Environment[entry[..equals]]=entry[(equals+1)..];
                }
            }
            // No text redirection/pump: the child inherits the host OS handles.
            // Translated FILE redirection requires the separate descriptor API.
            using Process? child=Process.Start(info);
            if(child is null){setErrno(5);return query?0:-1;}
            child.WaitForExit();
            return query?(child.ExitCode==0?1:0):child.ExitCode;
        }
        catch(DecoderFallbackException){setErrno(84);}
        catch(OverflowException){setErrno(7);}
        catch(Win32Exception error){setErrno(NativeError(error.NativeErrorCode));}
        catch(FileNotFoundException){setErrno(2);}
        catch(DirectoryNotFoundException){setErrno(2);}
        catch(UnauthorizedAccessException){setErrno(13);}
        catch(PathTooLongException){setErrno(36);}
        catch(PlatformNotSupportedException){setErrno(38);}
        catch(NotSupportedException){setErrno(38);}
        catch(ArgumentException){setErrno(22);}
        catch(IOException){setErrno(5);}
        catch(OutOfMemoryException){setErrno(12);}
        return query?0:-1;
    }

    public void Dispose()=>Volatile.Write(ref disposed,true);
}
