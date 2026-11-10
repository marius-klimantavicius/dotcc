// Generated from config/opengl.json by scripts/generate-gl.py.
// CUI calls managed thunks; only these thunks invoke native GL addresses.
#nullable enable
using System;
using System.Runtime.InteropServices;
namespace Managed.Cui;
internal static unsafe class OpenGlHost
{
    private static delegate* unmanaged<uint, void> native_glEnable;
    private static void glEnable(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glEnable == null) throw new InvalidOperationException("OpenGL entry point glEnable must be loaded for the current app.");
        native_glEnable(a0);
    }
    private static delegate* unmanaged<uint, void> native_glDisable;
    private static void glDisable(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glDisable == null) throw new InvalidOperationException("OpenGL entry point glDisable must be loaded for the current app.");
        native_glDisable(a0);
    }
    private static delegate* unmanaged<uint, uint, void> native_glBlendFunc;
    private static void glBlendFunc(uint a0, uint a1)
    {
        GlfwHost.CheckContext();
        if (native_glBlendFunc == null) throw new InvalidOperationException("OpenGL entry point glBlendFunc must be loaded for the current app.");
        native_glBlendFunc(a0, a1);
    }
    private static delegate* unmanaged<uint, void> native_glBlendEquation;
    private static void glBlendEquation(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glBlendEquation == null) throw new InvalidOperationException("OpenGL entry point glBlendEquation must be loaded for the current app.");
        native_glBlendEquation(a0);
    }
    private static delegate* unmanaged<int, int, int, int, void> native_glViewport;
    private static void glViewport(int a0, int a1, int a2, int a3)
    {
        GlfwHost.CheckContext();
        if (native_glViewport == null) throw new InvalidOperationException("OpenGL entry point glViewport must be loaded for the current app.");
        native_glViewport(a0, a1, a2, a3);
    }
    private static delegate* unmanaged<int, int, int, int, void> native_glScissor;
    private static void glScissor(int a0, int a1, int a2, int a3)
    {
        GlfwHost.CheckContext();
        if (native_glScissor == null) throw new InvalidOperationException("OpenGL entry point glScissor must be loaded for the current app.");
        native_glScissor(a0, a1, a2, a3);
    }
    private static delegate* unmanaged<float, float, float, float, void> native_glClearColor;
    private static void glClearColor(float a0, float a1, float a2, float a3)
    {
        GlfwHost.CheckContext();
        if (native_glClearColor == null) throw new InvalidOperationException("OpenGL entry point glClearColor must be loaded for the current app.");
        native_glClearColor(a0, a1, a2, a3);
    }
    private static delegate* unmanaged<uint, void> native_glClear;
    private static void glClear(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glClear == null) throw new InvalidOperationException("OpenGL entry point glClear must be loaded for the current app.");
        native_glClear(a0);
    }
    private static delegate* unmanaged<int, uint*, void> native_glGenTextures;
    private static void glGenTextures(int a0, uint* a1)
    {
        GlfwHost.CheckContext();
        if (native_glGenTextures == null) throw new InvalidOperationException("OpenGL entry point glGenTextures must be loaded for the current app.");
        native_glGenTextures(a0, a1);
    }
    private static delegate* unmanaged<int, uint*, void> native_glDeleteTextures;
    private static void glDeleteTextures(int a0, uint* a1)
    {
        GlfwHost.CheckContext();
        if (native_glDeleteTextures == null) throw new InvalidOperationException("OpenGL entry point glDeleteTextures must be loaded for the current app.");
        native_glDeleteTextures(a0, a1);
    }
    private static delegate* unmanaged<uint, uint, void> native_glBindTexture;
    private static void glBindTexture(uint a0, uint a1)
    {
        GlfwHost.CheckContext();
        if (native_glBindTexture == null) throw new InvalidOperationException("OpenGL entry point glBindTexture must be loaded for the current app.");
        native_glBindTexture(a0, a1);
    }
    private static delegate* unmanaged<uint, uint, int, void> native_glTexParameteri;
    private static void glTexParameteri(uint a0, uint a1, int a2)
    {
        GlfwHost.CheckContext();
        if (native_glTexParameteri == null) throw new InvalidOperationException("OpenGL entry point glTexParameteri must be loaded for the current app.");
        native_glTexParameteri(a0, a1, a2);
    }
    private static delegate* unmanaged<uint, int, int, int, int, int, uint, uint, void*, void> native_glTexImage2D;
    private static void glTexImage2D(uint a0, int a1, int a2, int a3, int a4, int a5, uint a6, uint a7, void* a8)
    {
        GlfwHost.CheckContext();
        if (native_glTexImage2D == null) throw new InvalidOperationException("OpenGL entry point glTexImage2D must be loaded for the current app.");
        native_glTexImage2D(a0, a1, a2, a3, a4, a5, a6, a7, a8);
    }
    private static delegate* unmanaged<uint, int, int, int, int, int, uint, uint, void*, void> native_glTexSubImage2D;
    private static void glTexSubImage2D(uint a0, int a1, int a2, int a3, int a4, int a5, uint a6, uint a7, void* a8)
    {
        GlfwHost.CheckContext();
        if (native_glTexSubImage2D == null) throw new InvalidOperationException("OpenGL entry point glTexSubImage2D must be loaded for the current app.");
        native_glTexSubImage2D(a0, a1, a2, a3, a4, a5, a6, a7, a8);
    }
    private static delegate* unmanaged<uint, int, void> native_glPixelStorei;
    private static void glPixelStorei(uint a0, int a1)
    {
        GlfwHost.CheckContext();
        if (native_glPixelStorei == null) throw new InvalidOperationException("OpenGL entry point glPixelStorei must be loaded for the current app.");
        native_glPixelStorei(a0, a1);
    }
    private static delegate* unmanaged<uint, void> native_glActiveTexture;
    private static void glActiveTexture(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glActiveTexture == null) throw new InvalidOperationException("OpenGL entry point glActiveTexture must be loaded for the current app.");
        native_glActiveTexture(a0);
    }
    private static delegate* unmanaged<uint, int, uint, void*, void> native_glDrawElements;
    private static void glDrawElements(uint a0, int a1, uint a2, void* a3)
    {
        GlfwHost.CheckContext();
        if (native_glDrawElements == null) throw new InvalidOperationException("OpenGL entry point glDrawElements must be loaded for the current app.");
        native_glDrawElements(a0, a1, a2, a3);
    }
    private static delegate* unmanaged<uint, byte*> native_glGetString;
    private static byte* glGetString(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glGetString == null) throw new InvalidOperationException("OpenGL entry point glGetString must be loaded for the current app.");
        return native_glGetString(a0);
    }
    private static delegate* unmanaged<int, uint*, void> native_glGenBuffers;
    private static void glGenBuffers(int a0, uint* a1)
    {
        GlfwHost.CheckContext();
        if (native_glGenBuffers == null) throw new InvalidOperationException("OpenGL entry point glGenBuffers must be loaded for the current app.");
        native_glGenBuffers(a0, a1);
    }
    private static delegate* unmanaged<int, uint*, void> native_glDeleteBuffers;
    private static void glDeleteBuffers(int a0, uint* a1)
    {
        GlfwHost.CheckContext();
        if (native_glDeleteBuffers == null) throw new InvalidOperationException("OpenGL entry point glDeleteBuffers must be loaded for the current app.");
        native_glDeleteBuffers(a0, a1);
    }
    private static delegate* unmanaged<uint, uint, void> native_glBindBuffer;
    private static void glBindBuffer(uint a0, uint a1)
    {
        GlfwHost.CheckContext();
        if (native_glBindBuffer == null) throw new InvalidOperationException("OpenGL entry point glBindBuffer must be loaded for the current app.");
        native_glBindBuffer(a0, a1);
    }
    private static delegate* unmanaged<uint, nint, void*, uint, void> native_glBufferData;
    private static void glBufferData(uint a0, long a1, void* a2, uint a3)
    {
        GlfwHost.CheckContext();
        if (native_glBufferData == null) throw new InvalidOperationException("OpenGL entry point glBufferData must be loaded for the current app.");
        native_glBufferData(a0, (nint)a1, a2, a3);
    }
    private static delegate* unmanaged<int, uint*, void> native_glGenVertexArrays;
    private static void glGenVertexArrays(int a0, uint* a1)
    {
        GlfwHost.CheckContext();
        if (native_glGenVertexArrays == null) throw new InvalidOperationException("OpenGL entry point glGenVertexArrays must be loaded for the current app.");
        native_glGenVertexArrays(a0, a1);
    }
    private static delegate* unmanaged<int, uint*, void> native_glDeleteVertexArrays;
    private static void glDeleteVertexArrays(int a0, uint* a1)
    {
        GlfwHost.CheckContext();
        if (native_glDeleteVertexArrays == null) throw new InvalidOperationException("OpenGL entry point glDeleteVertexArrays must be loaded for the current app.");
        native_glDeleteVertexArrays(a0, a1);
    }
    private static delegate* unmanaged<uint, void> native_glBindVertexArray;
    private static void glBindVertexArray(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glBindVertexArray == null) throw new InvalidOperationException("OpenGL entry point glBindVertexArray must be loaded for the current app.");
        native_glBindVertexArray(a0);
    }
    private static delegate* unmanaged<uint, void> native_glEnableVertexAttribArray;
    private static void glEnableVertexAttribArray(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glEnableVertexAttribArray == null) throw new InvalidOperationException("OpenGL entry point glEnableVertexAttribArray must be loaded for the current app.");
        native_glEnableVertexAttribArray(a0);
    }
    private static delegate* unmanaged<uint, int, uint, byte, int, void*, void> native_glVertexAttribPointer;
    private static void glVertexAttribPointer(uint a0, int a1, uint a2, byte a3, int a4, void* a5)
    {
        GlfwHost.CheckContext();
        if (native_glVertexAttribPointer == null) throw new InvalidOperationException("OpenGL entry point glVertexAttribPointer must be loaded for the current app.");
        native_glVertexAttribPointer(a0, a1, a2, a3, a4, a5);
    }
    private static delegate* unmanaged<uint, uint> native_glCreateShader;
    private static uint glCreateShader(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glCreateShader == null) throw new InvalidOperationException("OpenGL entry point glCreateShader must be loaded for the current app.");
        return native_glCreateShader(a0);
    }
    private static delegate* unmanaged<uint, int, byte**, int*, void> native_glShaderSource;
    private static void glShaderSource(uint a0, int a1, byte** a2, int* a3)
    {
        GlfwHost.CheckContext();
        if (native_glShaderSource == null) throw new InvalidOperationException("OpenGL entry point glShaderSource must be loaded for the current app.");
        native_glShaderSource(a0, a1, a2, a3);
    }
    private static delegate* unmanaged<uint, void> native_glCompileShader;
    private static void glCompileShader(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glCompileShader == null) throw new InvalidOperationException("OpenGL entry point glCompileShader must be loaded for the current app.");
        native_glCompileShader(a0);
    }
    private static delegate* unmanaged<uint, uint, int*, void> native_glGetShaderiv;
    private static void glGetShaderiv(uint a0, uint a1, int* a2)
    {
        GlfwHost.CheckContext();
        if (native_glGetShaderiv == null) throw new InvalidOperationException("OpenGL entry point glGetShaderiv must be loaded for the current app.");
        native_glGetShaderiv(a0, a1, a2);
    }
    private static delegate* unmanaged<uint, int, int*, byte*, void> native_glGetShaderInfoLog;
    private static void glGetShaderInfoLog(uint a0, int a1, int* a2, byte* a3)
    {
        GlfwHost.CheckContext();
        if (native_glGetShaderInfoLog == null) throw new InvalidOperationException("OpenGL entry point glGetShaderInfoLog must be loaded for the current app.");
        native_glGetShaderInfoLog(a0, a1, a2, a3);
    }
    private static delegate* unmanaged<uint, void> native_glDeleteShader;
    private static void glDeleteShader(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glDeleteShader == null) throw new InvalidOperationException("OpenGL entry point glDeleteShader must be loaded for the current app.");
        native_glDeleteShader(a0);
    }
    private static delegate* unmanaged<uint> native_glCreateProgram;
    private static uint glCreateProgram()
    {
        GlfwHost.CheckContext();
        if (native_glCreateProgram == null) throw new InvalidOperationException("OpenGL entry point glCreateProgram must be loaded for the current app.");
        return native_glCreateProgram();
    }
    private static delegate* unmanaged<uint, uint, void> native_glAttachShader;
    private static void glAttachShader(uint a0, uint a1)
    {
        GlfwHost.CheckContext();
        if (native_glAttachShader == null) throw new InvalidOperationException("OpenGL entry point glAttachShader must be loaded for the current app.");
        native_glAttachShader(a0, a1);
    }
    private static delegate* unmanaged<uint, void> native_glLinkProgram;
    private static void glLinkProgram(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glLinkProgram == null) throw new InvalidOperationException("OpenGL entry point glLinkProgram must be loaded for the current app.");
        native_glLinkProgram(a0);
    }
    private static delegate* unmanaged<uint, uint, int*, void> native_glGetProgramiv;
    private static void glGetProgramiv(uint a0, uint a1, int* a2)
    {
        GlfwHost.CheckContext();
        if (native_glGetProgramiv == null) throw new InvalidOperationException("OpenGL entry point glGetProgramiv must be loaded for the current app.");
        native_glGetProgramiv(a0, a1, a2);
    }
    private static delegate* unmanaged<uint, int, int*, byte*, void> native_glGetProgramInfoLog;
    private static void glGetProgramInfoLog(uint a0, int a1, int* a2, byte* a3)
    {
        GlfwHost.CheckContext();
        if (native_glGetProgramInfoLog == null) throw new InvalidOperationException("OpenGL entry point glGetProgramInfoLog must be loaded for the current app.");
        native_glGetProgramInfoLog(a0, a1, a2, a3);
    }
    private static delegate* unmanaged<uint, void> native_glDeleteProgram;
    private static void glDeleteProgram(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glDeleteProgram == null) throw new InvalidOperationException("OpenGL entry point glDeleteProgram must be loaded for the current app.");
        native_glDeleteProgram(a0);
    }
    private static delegate* unmanaged<uint, void> native_glUseProgram;
    private static void glUseProgram(uint a0)
    {
        GlfwHost.CheckContext();
        if (native_glUseProgram == null) throw new InvalidOperationException("OpenGL entry point glUseProgram must be loaded for the current app.");
        native_glUseProgram(a0);
    }
    private static delegate* unmanaged<uint, byte*, int> native_glGetUniformLocation;
    private static int glGetUniformLocation(uint a0, byte* a1)
    {
        GlfwHost.CheckContext();
        if (native_glGetUniformLocation == null) throw new InvalidOperationException("OpenGL entry point glGetUniformLocation must be loaded for the current app.");
        return native_glGetUniformLocation(a0, a1);
    }
    private static delegate* unmanaged<int, int, void> native_glUniform1i;
    private static void glUniform1i(int a0, int a1)
    {
        GlfwHost.CheckContext();
        if (native_glUniform1i == null) throw new InvalidOperationException("OpenGL entry point glUniform1i must be loaded for the current app.");
        native_glUniform1i(a0, a1);
    }
    private static delegate* unmanaged<int, float, float, void> native_glUniform2f;
    private static void glUniform2f(int a0, float a1, float a2)
    {
        GlfwHost.CheckContext();
        if (native_glUniform2f == null) throw new InvalidOperationException("OpenGL entry point glUniform2f must be loaded for the current app.");
        native_glUniform2f(a0, a1, a2);
    }
    private static delegate* unmanaged<uint, uint, byte*, void> native_glBindAttribLocation;
    private static void glBindAttribLocation(uint a0, uint a1, byte* a2)
    {
        GlfwHost.CheckContext();
        if (native_glBindAttribLocation == null) throw new InvalidOperationException("OpenGL entry point glBindAttribLocation must be loaded for the current app.");
        native_glBindAttribLocation(a0, a1, a2);
    }
    private static delegate* unmanaged<int, int, int, int, uint, uint, void*, void> native_glReadPixels;
    private static void glReadPixels(int a0, int a1, int a2, int a3, uint a4, uint a5, void* a6)
    {
        GlfwHost.CheckContext();
        if (native_glReadPixels == null) throw new InvalidOperationException("OpenGL entry point glReadPixels must be loaded for the current app.");
        native_glReadPixels(a0, a1, a2, a3, a4, a5, a6);
    }
    private static delegate* unmanaged<void> native_glFinish;
    private static void glFinish()
    {
        GlfwHost.CheckContext();
        if (native_glFinish == null) throw new InvalidOperationException("OpenGL entry point glFinish must be loaded for the current app.");
        native_glFinish();
    }
    internal static void Reset()
    {
        native_glEnable = null;
        native_glDisable = null;
        native_glBlendFunc = null;
        native_glBlendEquation = null;
        native_glViewport = null;
        native_glScissor = null;
        native_glClearColor = null;
        native_glClear = null;
        native_glGenTextures = null;
        native_glDeleteTextures = null;
        native_glBindTexture = null;
        native_glTexParameteri = null;
        native_glTexImage2D = null;
        native_glTexSubImage2D = null;
        native_glPixelStorei = null;
        native_glActiveTexture = null;
        native_glDrawElements = null;
        native_glGetString = null;
        native_glGenBuffers = null;
        native_glDeleteBuffers = null;
        native_glBindBuffer = null;
        native_glBufferData = null;
        native_glGenVertexArrays = null;
        native_glDeleteVertexArrays = null;
        native_glBindVertexArray = null;
        native_glEnableVertexAttribArray = null;
        native_glVertexAttribPointer = null;
        native_glCreateShader = null;
        native_glShaderSource = null;
        native_glCompileShader = null;
        native_glGetShaderiv = null;
        native_glGetShaderInfoLog = null;
        native_glDeleteShader = null;
        native_glCreateProgram = null;
        native_glAttachShader = null;
        native_glLinkProgram = null;
        native_glGetProgramiv = null;
        native_glGetProgramInfoLog = null;
        native_glDeleteProgram = null;
        native_glUseProgram = null;
        native_glGetUniformLocation = null;
        native_glUniform1i = null;
        native_glUniform2f = null;
        native_glBindAttribLocation = null;
        native_glReadPixels = null;
        native_glFinish = null;
    }
    public static void* GetProc(byte* utf8)
    {
        GlfwHost.CheckThread();
        if (utf8 == null || global::Glfw.Glfw.glfwGetCurrentContext() is null) return null;
        string? name = Marshal.PtrToStringUTF8((nint)utf8);
        if (name is null) return null;
        nint address = global::Glfw.Glfw.glfwGetProcAddress(name);
        if (address == 0) return null;
        switch (name)
        {
            case "glEnable":
                native_glEnable = (delegate* unmanaged<uint, void>)address;
                return (void*)(delegate*<uint, void>)&glEnable;
            case "glDisable":
                native_glDisable = (delegate* unmanaged<uint, void>)address;
                return (void*)(delegate*<uint, void>)&glDisable;
            case "glBlendFunc":
                native_glBlendFunc = (delegate* unmanaged<uint, uint, void>)address;
                return (void*)(delegate*<uint, uint, void>)&glBlendFunc;
            case "glBlendEquation":
                native_glBlendEquation = (delegate* unmanaged<uint, void>)address;
                return (void*)(delegate*<uint, void>)&glBlendEquation;
            case "glViewport":
                native_glViewport = (delegate* unmanaged<int, int, int, int, void>)address;
                return (void*)(delegate*<int, int, int, int, void>)&glViewport;
            case "glScissor":
                native_glScissor = (delegate* unmanaged<int, int, int, int, void>)address;
                return (void*)(delegate*<int, int, int, int, void>)&glScissor;
            case "glClearColor":
                native_glClearColor = (delegate* unmanaged<float, float, float, float, void>)address;
                return (void*)(delegate*<float, float, float, float, void>)&glClearColor;
            case "glClear":
                native_glClear = (delegate* unmanaged<uint, void>)address;
                return (void*)(delegate*<uint, void>)&glClear;
            case "glGenTextures":
                native_glGenTextures = (delegate* unmanaged<int, uint*, void>)address;
                return (void*)(delegate*<int, uint*, void>)&glGenTextures;
            case "glDeleteTextures":
                native_glDeleteTextures = (delegate* unmanaged<int, uint*, void>)address;
                return (void*)(delegate*<int, uint*, void>)&glDeleteTextures;
            case "glBindTexture":
                native_glBindTexture = (delegate* unmanaged<uint, uint, void>)address;
                return (void*)(delegate*<uint, uint, void>)&glBindTexture;
            case "glTexParameteri":
                native_glTexParameteri = (delegate* unmanaged<uint, uint, int, void>)address;
                return (void*)(delegate*<uint, uint, int, void>)&glTexParameteri;
            case "glTexImage2D":
                native_glTexImage2D = (delegate* unmanaged<uint, int, int, int, int, int, uint, uint, void*, void>)address;
                return (void*)(delegate*<uint, int, int, int, int, int, uint, uint, void*, void>)&glTexImage2D;
            case "glTexSubImage2D":
                native_glTexSubImage2D = (delegate* unmanaged<uint, int, int, int, int, int, uint, uint, void*, void>)address;
                return (void*)(delegate*<uint, int, int, int, int, int, uint, uint, void*, void>)&glTexSubImage2D;
            case "glPixelStorei":
                native_glPixelStorei = (delegate* unmanaged<uint, int, void>)address;
                return (void*)(delegate*<uint, int, void>)&glPixelStorei;
            case "glActiveTexture":
                native_glActiveTexture = (delegate* unmanaged<uint, void>)address;
                return (void*)(delegate*<uint, void>)&glActiveTexture;
            case "glDrawElements":
                native_glDrawElements = (delegate* unmanaged<uint, int, uint, void*, void>)address;
                return (void*)(delegate*<uint, int, uint, void*, void>)&glDrawElements;
            case "glGetString":
                native_glGetString = (delegate* unmanaged<uint, byte*>)address;
                return (void*)(delegate*<uint, byte*>)&glGetString;
            case "glGenBuffers":
                native_glGenBuffers = (delegate* unmanaged<int, uint*, void>)address;
                return (void*)(delegate*<int, uint*, void>)&glGenBuffers;
            case "glDeleteBuffers":
                native_glDeleteBuffers = (delegate* unmanaged<int, uint*, void>)address;
                return (void*)(delegate*<int, uint*, void>)&glDeleteBuffers;
            case "glBindBuffer":
                native_glBindBuffer = (delegate* unmanaged<uint, uint, void>)address;
                return (void*)(delegate*<uint, uint, void>)&glBindBuffer;
            case "glBufferData":
                native_glBufferData = (delegate* unmanaged<uint, nint, void*, uint, void>)address;
                return (void*)(delegate*<uint, long, void*, uint, void>)&glBufferData;
            case "glGenVertexArrays":
                native_glGenVertexArrays = (delegate* unmanaged<int, uint*, void>)address;
                return (void*)(delegate*<int, uint*, void>)&glGenVertexArrays;
            case "glDeleteVertexArrays":
                native_glDeleteVertexArrays = (delegate* unmanaged<int, uint*, void>)address;
                return (void*)(delegate*<int, uint*, void>)&glDeleteVertexArrays;
            case "glBindVertexArray":
                native_glBindVertexArray = (delegate* unmanaged<uint, void>)address;
                return (void*)(delegate*<uint, void>)&glBindVertexArray;
            case "glEnableVertexAttribArray":
                native_glEnableVertexAttribArray = (delegate* unmanaged<uint, void>)address;
                return (void*)(delegate*<uint, void>)&glEnableVertexAttribArray;
            case "glVertexAttribPointer":
                native_glVertexAttribPointer = (delegate* unmanaged<uint, int, uint, byte, int, void*, void>)address;
                return (void*)(delegate*<uint, int, uint, byte, int, void*, void>)&glVertexAttribPointer;
            case "glCreateShader":
                native_glCreateShader = (delegate* unmanaged<uint, uint>)address;
                return (void*)(delegate*<uint, uint>)&glCreateShader;
            case "glShaderSource":
                native_glShaderSource = (delegate* unmanaged<uint, int, byte**, int*, void>)address;
                return (void*)(delegate*<uint, int, byte**, int*, void>)&glShaderSource;
            case "glCompileShader":
                native_glCompileShader = (delegate* unmanaged<uint, void>)address;
                return (void*)(delegate*<uint, void>)&glCompileShader;
            case "glGetShaderiv":
                native_glGetShaderiv = (delegate* unmanaged<uint, uint, int*, void>)address;
                return (void*)(delegate*<uint, uint, int*, void>)&glGetShaderiv;
            case "glGetShaderInfoLog":
                native_glGetShaderInfoLog = (delegate* unmanaged<uint, int, int*, byte*, void>)address;
                return (void*)(delegate*<uint, int, int*, byte*, void>)&glGetShaderInfoLog;
            case "glDeleteShader":
                native_glDeleteShader = (delegate* unmanaged<uint, void>)address;
                return (void*)(delegate*<uint, void>)&glDeleteShader;
            case "glCreateProgram":
                native_glCreateProgram = (delegate* unmanaged<uint>)address;
                return (void*)(delegate*<uint>)&glCreateProgram;
            case "glAttachShader":
                native_glAttachShader = (delegate* unmanaged<uint, uint, void>)address;
                return (void*)(delegate*<uint, uint, void>)&glAttachShader;
            case "glLinkProgram":
                native_glLinkProgram = (delegate* unmanaged<uint, void>)address;
                return (void*)(delegate*<uint, void>)&glLinkProgram;
            case "glGetProgramiv":
                native_glGetProgramiv = (delegate* unmanaged<uint, uint, int*, void>)address;
                return (void*)(delegate*<uint, uint, int*, void>)&glGetProgramiv;
            case "glGetProgramInfoLog":
                native_glGetProgramInfoLog = (delegate* unmanaged<uint, int, int*, byte*, void>)address;
                return (void*)(delegate*<uint, int, int*, byte*, void>)&glGetProgramInfoLog;
            case "glDeleteProgram":
                native_glDeleteProgram = (delegate* unmanaged<uint, void>)address;
                return (void*)(delegate*<uint, void>)&glDeleteProgram;
            case "glUseProgram":
                native_glUseProgram = (delegate* unmanaged<uint, void>)address;
                return (void*)(delegate*<uint, void>)&glUseProgram;
            case "glGetUniformLocation":
                native_glGetUniformLocation = (delegate* unmanaged<uint, byte*, int>)address;
                return (void*)(delegate*<uint, byte*, int>)&glGetUniformLocation;
            case "glUniform1i":
                native_glUniform1i = (delegate* unmanaged<int, int, void>)address;
                return (void*)(delegate*<int, int, void>)&glUniform1i;
            case "glUniform2f":
                native_glUniform2f = (delegate* unmanaged<int, float, float, void>)address;
                return (void*)(delegate*<int, float, float, void>)&glUniform2f;
            case "glBindAttribLocation":
                native_glBindAttribLocation = (delegate* unmanaged<uint, uint, byte*, void>)address;
                return (void*)(delegate*<uint, uint, byte*, void>)&glBindAttribLocation;
            case "glReadPixels":
                native_glReadPixels = (delegate* unmanaged<int, int, int, int, uint, uint, void*, void>)address;
                return (void*)(delegate*<int, int, int, int, uint, uint, void*, void>)&glReadPixels;
            case "glFinish":
                native_glFinish = (delegate* unmanaged<void>)address;
                return (void*)(delegate*<void>)&glFinish;
            default: return null;
        }
    }
}
