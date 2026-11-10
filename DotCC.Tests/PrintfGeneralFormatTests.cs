#nullable enable

using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

public sealed unsafe class PrintfGeneralFormatTests
{
    [Theory]
    [InlineData("%g", 3.141592653589793, "3.14159")]
    [InlineData("%G", 3.141592653589793, "3.14159")]
    [InlineData("%.0g", 18.4, "2e+01")]
    [InlineData("%.0G", 18.4, "2E+01")]
    [InlineData("%.g", 1.25, "1")]
    [InlineData("%.1g", 1.25, "1")]
    [InlineData("%.3g", 12.5, "12.5")]
    [InlineData("%.8G", 123.456789, "123.45679")]
    [InlineData("%.17g", 18.399999618530273, "18.399999618530273")]
    [InlineData("%g", 999999.0, "999999")]
    [InlineData("%g", 1000000.0, "1e+06")]
    [InlineData("%G", 1000000.0, "1E+06")]
    [InlineData("%g", 999999.5, "1e+06")]
    [InlineData("%.3g", 999.5, "1e+03")]
    [InlineData("%.3G", 999.5, "1E+03")]
    [InlineData("%g", 0.0001, "0.0001")]
    [InlineData("%g", 0.00001, "1e-05")]
    [InlineData("%G", 0.00001, "1E-05")]
    [InlineData("%g", 0.00009999996, "0.0001")]
    [InlineData("%.6g", 1.25, "1.25")]
    [InlineData("%g", 0.0, "0")]
    [InlineData("%g", -0.0, "-0")]
    [InlineData("%g", -1234567.0, "-1.23457e+06")]
    [InlineData("%+12.4g", 18.399999618530273, "       +18.4")]
    public void General_precision_and_notation_match_C(string format, double value, string expected)
    {
        byte* output = stackalloc byte[128];
        fixed (byte* pattern = Encoding.UTF8.GetBytes(format + '\0'))
        {
            snprintf(output, 128, pattern).Arg(value).Done().ShouldBe(expected.Length);
            Encoding.UTF8.GetString(output, expected.Length).ShouldBe(expected);
            output[expected.Length].ShouldBe((byte)0);

            vsnprintf(output, 128, pattern, new VaList(new VaArg[] { value })).ShouldBe(expected.Length);
            Encoding.UTF8.GetString(output, expected.Length).ShouldBe(expected);
            output[expected.Length].ShouldBe((byte)0);
        }
    }

    [Fact]
    public void Float_promoted_to_double_uses_six_significant_digits_for_gallery_label()
    {
        byte* output = stackalloc byte[64];
        float used = 18.4f;
        double promoted = used;
        var format = L("%g / %g GB\0"u8);
        snprintf(output, 64, format).Arg(used).Arg(20.0f).Done().ShouldBe(12);
        Encoding.UTF8.GetString(output, 12).ShouldBe("18.4 / 20 GB");
        snprintf(output, 64, format).Arg(promoted).Arg(20.0).Done().ShouldBe(12);
        Encoding.UTF8.GetString(output, 12).ShouldBe("18.4 / 20 GB");
    }

    [Theory]
    [InlineData(-1, "3.14159")]
    [InlineData(0, "3")]
    [InlineData(4, "3.142")]
    public void Dynamic_precision_obeys_general_format_defaults(int precision, string expected)
    {
        byte* output = stackalloc byte[32];
        var format = L("%.*g\0"u8);
        snprintf(output, 32, format).Arg(precision).Arg(3.141592653589793).Done().ShouldBe(expected.Length);
        Encoding.UTF8.GetString(output, expected.Length).ShouldBe(expected);
        vsnprintf(output, 32, format, new VaList(new VaArg[] { precision, 3.141592653589793 }))
            .ShouldBe(expected.Length);
        Encoding.UTF8.GetString(output, expected.Length).ShouldBe(expected);
    }
}
