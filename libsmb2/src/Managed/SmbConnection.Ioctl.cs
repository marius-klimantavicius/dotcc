using System.Runtime.InteropServices;
using static Managed.Smb.LibSmb2;

namespace Managed.Smb;

public sealed partial class SmbConnection
{
    internal Task<byte[]> IoctlAsync(uint controlCode, byte[] input, uint maxOutputResponse,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (maxOutputResponse == 0 || maxOutputResponse > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(maxOutputResponse));
        return RunOperationAsync("ioctl", op => StartIoctl(op, controlCode, input, maxOutputResponse),
            op => ReadIoctl(op, controlCode, maxOutputResponse), cancellationToken, Ownership.IoctlOutput);
    }

    private unsafe int StartIoctl(Operation op, uint controlCode, byte[] input, uint maxOutput)
    {
        smb2_ioctl_request request = default;
        request.ctl_code = controlCode;
        request.input_count = checked((uint)input.Length);
        request.max_output_response = maxOutput;
        request.flags = SMB2_0_IOCTL_IS_FSCTL;
        for (int i = 0; i < SMB2_FD_SIZE; i++) request.file_id[i] = byte.MaxValue;
        request.input = (byte*)op.Allocate(input.Length);
        input.CopyTo(new Span<byte>(request.input, input.Length));
        smb2_pdu* pdu = smb2_cmd_ioctl_async(Context, &request, &CompleteIoctl, op.Token);
        if (pdu == null) return -Libc.EIO;
        smb2_queue_pdu(Context, pdu);
        return 0;
    }

    private static unsafe byte[] ReadIoctl(Operation op, uint controlCode, uint maxOutput)
    {
        if (!op.HasIoctlReply || op.ControlCode != controlCode || op.DataLength > maxOutput ||
            (op.DataLength != 0 && op.Data == 0)) throw new InvalidDataException("Malformed SMB IOCTL response");
        return op.DataLength == 0 ? [] : new ReadOnlySpan<byte>((void*)op.Data, checked((int)op.DataLength)).ToArray();
    }

    private static unsafe void CompleteIoctl(smb2_context* context, int status, void* data, void* privateData)
    {
        try
        {
            var op = (Operation)GCHandle.FromIntPtr((nint)privateData).Target!;
            uint ntStatus = unchecked((uint)status);
            // Partial referrals must not be cached as complete answers.
            op.Status = ntStatus == SMB2_STATUS_SUCCESS ? 0 : -nterror_to_errno(ntStatus);
            if (op.Status == 0 && data != null)
            {
                var reply = (smb2_ioctl_reply*)data;
                op.HasIoctlReply = true;
                op.ControlCode = reply->ctl_code;
                op.DataLength = reply->output_count;
                op.Data = (nint)reply->output;
            }
            else
            {
                op.CallbackError = CaptureError(context, op.Name, op.Status, ntStatus);
                // The decoder owns the reply container; callers own its output allocation.
                if (data != null && ntStatus == SMB2_STATUS_BUFFER_OVERFLOW)
                    smb2_free_data(context, ((smb2_ioctl_reply*)data)->output);
            }
            op.Finished = true;
            op.Completion.TrySetResult();
        }
        catch (Exception error) { CallbackFailed((nint)context, error); }
    }

    private static unsafe SmbException CaptureError(smb2_context* context, string name, int status, uint? ntStatus = null)
    {
        uint raw = ntStatus ?? unchecked((uint)smb2_get_nterror(context));
        string message = Marshal.PtrToStringUTF8((nint)smb2_get_error(context)) ?? "SMB operation failed";
        if (ntStatus.HasValue) message = Marshal.PtrToStringUTF8((nint)nterror_to_str(raw)) ?? message;
        return new SmbException(name, status, message, raw == 0 ? null : raw);
    }
}
