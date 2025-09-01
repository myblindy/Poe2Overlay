using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Graphics.Gdi;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Poe2Overlay;
static class ImageListener
{
    public static Action<int, TimeSpan>? PowerChargeUpdated;

    const int captureHeight = 120;
    const int startX = 33, startY = 23;
    const int endX = 83, endY = 64, nextX = 130;
    const int maskSize = 9;
    static int maskThreshold = 33;
    const int chargeCountOffsetX = 17, chargeCountOffsetY = 50, chargeCountEndX = 30, chargeCountEndY = 63;

    #region Masks
    static readonly Bgra32[][] powerChargeMasks = [
        [new(40, 60, 191, 255), new(49, 87, 240, 255), new(53, 120, 254, 255), new(58, 151, 255, 255), new(58, 160, 255, 255), new(61, 148, 255, 255), new(55, 107, 255, 255), new(53, 77, 248, 255), new(39, 52, 202, 255), new(46, 90, 233, 255), new(49, 133, 253, 255), new(61, 203, 255, 255), new(77, 237, 255, 255), new(72, 242, 255, 255), new(65, 228, 255, 255), new(56, 183, 255, 255), new(50, 115, 254, 255), new(44, 70, 240, 255), new(48, 117, 250, 255), new(69, 201, 255, 255), new(137, 253, 255, 255), new(172, 255, 255, 255), new(154, 255, 255, 255), new(168, 255, 255, 255), new(97, 244, 255, 255), new(63, 183, 255, 255), new(54, 106, 251, 255), new(66, 169, 255, 255), new(160, 245, 255, 255), new(240, 255, 255, 255), new(207, 255, 255, 255), new(189, 255, 255, 255), new(231, 255, 255, 255), new(204, 253, 255, 255), new(108, 223, 255, 255), new(99, 180, 254, 255), new(69, 194, 255, 255), new(168, 252, 255, 255), new(228, 255, 255, 255), new(139, 233, 250, 255), new(102, 202, 236, 255), new(165, 250, 254, 255), new(174, 255, 255, 255), new(108, 235, 255, 255), new(86, 178, 253, 255), new(66, 195, 255, 255), new(133, 252, 255, 255), new(192, 255, 255, 255), new(99, 213, 241, 255), new(47, 150, 211, 255), new(105, 242, 251, 255), new(157, 255, 255, 255), new(111, 238, 254, 255), new(67, 156, 247, 255), new(70, 185, 255, 255), new(157, 251, 255, 255), new(226, 255, 255, 255), new(173, 249, 254, 255), new(136, 235, 251, 255), new(154, 253, 255, 255), new(221, 255, 255, 255), new(154, 243, 254, 255), new(81, 152, 251, 255), new(74, 154, 255, 255), new(96, 234, 255, 255), new(184, 255, 255, 255), new(227, 255, 255, 255), new(220, 255, 255, 255), new(227, 255, 255, 255), new(214, 254, 255, 255), new(157, 225, 255, 255), new(76, 132, 253, 255), new(61, 102, 253, 255), new(69, 180, 255, 255), new(105, 249, 255, 255), new(155, 255, 255, 255), new(143, 255, 255, 255), new(154, 255, 255, 255), new(140, 236, 255, 255), new(73, 149, 255, 255), new(52, 84, 247, 255),],
        [new(36, 48, 162, 255),new(46, 78, 226, 255),new(52, 108, 252, 255),new(57, 143, 255, 255),new(57, 160, 255, 255),new(61, 154, 255, 255),new(56, 119, 255, 255),new(53, 82, 252, 255),new(44, 60, 219, 255),new(43, 75, 210, 255),new(48, 114, 249, 255),new(55, 181, 255, 255),new(73, 232, 255, 255),new(75, 242, 255, 255),new(67, 235, 255, 255),new(57, 198, 255, 255),new(52, 131, 255, 255),new(45, 78, 249, 255),new(44, 95, 239, 255),new(57, 169, 255, 255),new(112, 245, 255, 255),new(171, 255, 255, 255),new(153, 255, 255, 255),new(171, 255, 255, 255),new(116, 251, 255, 255),new(66, 202, 255, 255),new(57, 128, 255, 255),new(50, 125, 248, 255),new(116, 226, 255, 255),new(227, 255, 255, 255),new(223, 255, 255, 255),new(183, 255, 255, 255),new(218, 255, 255, 255),new(226, 255, 255, 255),new(122, 231, 255, 255),new(108, 205, 255, 255),new(50, 144, 253, 255),new(125, 241, 255, 255),new(226, 255, 255, 255),new(173, 247, 254, 255),new(97, 200, 237, 255),new(148, 239, 249, 255),new(189, 255, 255, 255),new(119, 243, 255, 255),new(95, 202, 253, 255),new(58, 161, 254, 255),new(102, 242, 255, 255),new(190, 255, 255, 255),new(138, 240, 251, 255),new(47, 149, 211, 255),new(85, 218, 241, 255),new(155, 255, 255, 255),new(122, 248, 255, 255),new(79, 184, 247, 255),new(65, 150, 253, 255),new(114, 236, 255, 255),new(221, 255, 255, 255),new(195, 253, 255, 255),new(142, 236, 251, 255),new(141, 249, 254, 255),new(213, 255, 255, 255),new(174, 251, 255, 255),new(101, 187, 251, 255),new(66, 116, 250, 255),new(83, 207, 255, 255),new(152, 255, 255, 255),new(225, 255, 255, 255),new(220, 255, 255, 255),new(226, 255, 255, 255),new(222, 254, 255, 255),new(174, 237, 255, 255),new(96, 164, 255, 255),new(58, 81, 240, 255),new(64, 146, 255, 255),new(90, 235, 255, 255),new(142, 255, 255, 255),new(148, 255, 255, 255),new(147, 255, 255, 255),new(154, 246, 255, 255),new(85, 168, 255, 255),new(55, 98, 253, 255),],
    ];
    static readonly L8[][] chargecount1Masks = [
        [new(0), new(0), new(0), new(29), new(28), new(0), new(0), new(12), new(117), new(207), new(141), new(0), new(0), new(15), new(77), new(148), new(153), new(0), new(0), new(0), new(0), new(86), new(151), new(0), new(0), new(0), new(0), new(88), new(151), new(0), new(0), new(0), new(0), new(88), new(151), new(0),],
        [new(0),new(0),new(0),new(11),new(40),new(9),new(0),new(0),new(61),new(171),new(208),new(43),new(0),new(0),new(53),new(97),new(199),new(49),new(0),new(0),new(0),new(9),new(179),new(50),new(0),new(0),new(0),new(13),new(180),new(50),new(0),new(0),new(0),new(13),new(180),new(50),],
        [new(0),new(0),new(26),new(31),new(0),new(0),new(9),new(107),new(202),new(157),new(1),new(0),new(13),new(73),new(137),new(168),new(2),new(0),new(0),new(0),new(69),new(165),new(2),new(0),new(0),new(0),new(72),new(165),new(2),new(0),new(0),new(0),new(72),new(165),new(2),new(0),],
        [new(0),new(0),new(8),new(40),new(10),new(0),new(0),new(51),new(164),new(215),new(50),new(0),new(0),new(47),new(93),new(199),new(56),new(0),new(0),new(0),new(2),new(174),new(57),new(0),new(0),new(0),new(6),new(175),new(57),new(0),new(0),new(0),new(6),new(175),new(57),new(0),],
    ];
    static readonly L8[][] chargeCount2Masks = [
        [new(0), new(0), new(32), new(51), new(28), new(0), new(0), new(33), new(184), new(196), new(185), new(28), new(0), new(20), new(61), new(43), new(197), new(51), new(0), new(0), new(0), new(70), new(191), new(32), new(0), new(0), new(36), new(176), new(96), new(0), new(0), new(18), new(172), new(207), new(107), new(60),],
        [new(0),new(0),new(12),new(48),new(44),new(8),new(0),new(0),new(122),new(195),new(209),new(108),new(0),new(1),new(57),new(33),new(134),new(160),new(0),new(0),new(0),new(8),new(171),new(122),new(0),new(0),new(4),new(114),new(168),new(21),new(0),new(0),new(88),new(228),new(142),new(98),],
        [new(0),new(28),new(51),new(31),new(0),new(0),new(28),new(175),new(194),new(194),new(33),new(0),new(17),new(63),new(36),new(195),new(58),new(0),new(0),new(0),new(56),new(197),new(38),new(0),new(0),new(28),new(169),new(110),new(0),new(0),new(14),new(157),new(215),new(111),new(60),new(0),],
    ];
    static readonly L8[][] chargecount3Masks = [
        [new(0), new(0), new(32), new(51), new(28), new(0), new(0), new(33), new(184), new(184), new(176), new(28), new(0), new(20), new(62), new(49), new(192), new(46), new(0), new(0), new(0), new(155), new(198), new(29), new(0), new(0), new(0), new(74), new(224), new(121), new(0), new(0), new(0), new(10), new(201), new(147),],
        [new(0),new(28),new(51),new(32),new(0),new(0),new(28),new(176),new(184),new(184),new(34),new(0),new(17),new(63),new(42),new(191),new(53),new(0),new(0),new(0),new(139),new(210),new(34),new(0),new(0),new(0),new(63),new(221),new(122),new(0),new(0),new(0),new(4),new(189),new(147),new(0),],
    ];
    static readonly L8[][] chargeCount4Masks = [
        [new(0), new(0), new(0), new(15), new(40), new(7), new(0), new(0), new(1), new(135), new(209), new(39), new(0), new(0), new(67), new(225), new(231), new(45), new(0), new(17), new(155), new(154), new(226), new(41), new(0), new(95), new(207), new(155), new(239), new(119), new(0), new(92), new(155), new(188), new(245), new(156),],
        [new(0),new(0),new(12),new(41),new(8),new(0),new(0),new(0),new(120),new(217),new(45),new(0),new(0),new(55),new(216),new(241),new(52),new(0),new(13),new(147),new(150),new(233),new(49),new(0),new(91),new(209),new(150),new(243),new(117),new(11),new(93),new(154),new(181),new(248),new(149),new(19),],
    ];
    static readonly L8[][] chargeCount5Masks = [
        [new(0), new(0), new(31), new(50), new(46), new(9), new(0), new(1), new(151), new(194), new(164), new(33), new(0), new(16), new(160), new(83), new(13), new(2), new(0), new(27), new(149), new(194), new(139), new(13), new(0), new(0), new(0), new(69), new(219), new(88), new(0), new(0), new(0), new(35), new(208), new(101),],
        [new(0),new(28),new(50),new(48),new(11),new(0),new(0),new(136),new(197),new(171),new(38),new(0),new(11),new(151),new(94),new(15),new(2),new(0),new(23),new(141),new(192),new(150),new(17),new(0),new(0),new(0),new(57),new(218),new(92),new(0),new(0),new(0),new(25),new(203),new(104),new(0),],
        [new(0),new(10),new(46),new(51),new(30),new(0),new(0),new(49),new(202),new(183),new(108),new(0),new(0),new(79),new(155),new(35),new(6),new(0),new(0),new(85),new(177),new(190),new(70),new(0),new(0),new(0),new(12),new(154),new(183),new(16),new(0),new(0),new(0),new(120),new(192),new(22),],
    ];
    static readonly L8[][] chargeCount6Masks = [[new(0), new(0), new(62), new(199), new(195), new(98), new(0), new(27), new(196), new(117), new(27), new(46), new(0), new(54), new(232), new(142), new(117), new(12), new(0), new(56), new(191), new(78), new(199), new(127), new(0), new(56), new(196), new(23), new(117), new(158), new(0), new(48), new(228), new(70), new(136), new(145),]];
    #endregion

    static Image<Bgra32> GetMaskImage(Image<Bgra32> img, int x)
    {
        var outputImage = new Image<Bgra32>(endX - startX, endY - startY);
        outputImage.Mutate(p => p
            .DrawImage(img, Point.Empty, new Rectangle(startX + x * (nextX - startX - x / 3), startY, endX - startX, endY - startY), 1f)
            .Crop(new(2, 2, endX - startX - 4, endY - startY - 4))
            .Resize(maskSize, maskSize, KnownResamplers.Robidoux));
        return outputImage;
    }

    static Image<L8> GetChargeCountImage(Image<Bgra32> img, int x)
    {
        var outputImage = new Image<L8>(chargeCountEndX - chargeCountOffsetX, chargeCountEndY - chargeCountOffsetY);
        outputImage.Mutate(p => p
            .DrawImage(img, Point.Empty,
                new Rectangle(startX + x * (nextX - startX - x / 3) + chargeCountOffsetX, startY + chargeCountOffsetY, chargeCountEndX - chargeCountOffsetX, chargeCountEndY - chargeCountOffsetY), 1f)
            .Resize(outputImage.Size.Width / 2, outputImage.Size.Height / 2, KnownResamplers.Robidoux));
        return outputImage;
    }

    static string DumpMaskToInitCode(Image<Bgra32> mask)
    {
        var sb = new System.Text.StringBuilder("[");
        mask.ProcessPixelRows(pa =>
        {
            for (int j = 0; j < maskSize; ++j)
            {
                var row = pa.GetRowSpan(j);
                for (int i = 0; i < maskSize; ++i)
                {
                    var p = row[i];
                    sb.Append($"new({p.R}, {p.G}, {p.B}, {p.A}),");
                }
            }
        });
        sb.Append("];");
        return sb.ToString();
    }

    static string DumpMaskToInitCode(Image<L8> mask)
    {
        var sb = new System.Text.StringBuilder("[");
        mask.ProcessPixelRows(pa =>
        {
            for (int j = 0; j < mask.Height; ++j)
            {
                var row = pa.GetRowSpan(j);
                for (int i = 0; i < mask.Width; ++i)
                {
                    var p = row[i];
                    sb.Append($"new({p.PackedValue}),");
                }
            }
        });
        sb.Append("];");
        return sb.ToString();
    }

    static bool TestMask(Image<Bgra32> img, int x)
    {
        img = GetMaskImage(img, x);

        foreach (var mask in powerChargeMasks)
            if (test(mask))
                return true;
        return false;

        bool test(Bgra32[] mask)
        {
            var success = true;
            img.ProcessPixelRows(pa =>
            {
                for (int j = 0; j < img.Height; ++j)
                {
                    var row = pa.GetRowSpan(j);
                    for (int i = 0; i < img.Width; ++i)
                    {
                        var p = row[i];
                        var m = mask[j * maskSize + i];
                        if (Math.Abs(p.R - m.R) > maskThreshold ||
                            Math.Abs(p.G - m.G) > maskThreshold ||
                            Math.Abs(p.B - m.B) > maskThreshold)
                        {
                            success = false;
                            return;
                        }
                    }
                }
            });

            return success;
        }
    }

    static int? TestCount(Image<Bgra32> img, int x)
    {
        var countImg = GetChargeCountImage(img, x);

        bool test(L8[] mask)
        {
            var success = true;
            countImg.ProcessPixelRows(pa =>
            {
                for (int j = 0; j < countImg.Height; ++j)
                {
                    var row = pa.GetRowSpan(j);
                    for (int i = 0; i < countImg.Width; ++i)
                    {
                        var p = row[i];
                        var m = mask[j * countImg.Width + i];
                        if (Math.Abs((int)p.PackedValue - (int)m.PackedValue) > maskThreshold)
                        {
                            success = false;
                            return;
                        }
                    }
                }
            });

            return success;
        }

        if (chargecount1Masks.Any(test))
            return 1;
        else if (chargeCount2Masks.Any(test))
            return 2;
        else if (chargecount3Masks.Any(test))
            return 3;
        else if (chargeCount4Masks.Any(test))
            return 4;
        else if (chargeCount5Masks.Any(test))
            return 5;
        else if (chargeCount6Masks.Any(test))
            return 6;
        return null;
    }

    public static Task StartAsync(CancellationToken ct) => Task.Run(async () =>
    {
        //var img = await Image.LoadAsync<Bgra32>("test/power-6.jpg").ConfigureAwait(false);
        //const int idx = 3;
        //GetMaskImage(img, idx).Save("test/tmp-m.png");
        //GetChargeCountImage(img, idx).Save("test/tmp-c.png");
        //var s = DumpMaskToInitCode(GetChargeCountImage(img, idx));
        //return;
        var monitor = PInvoke.MonitorFromPoint(new(-10000, -10000), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);
        MONITORINFO monitorInfo = new() { cbSize = (uint)Unsafe.SizeOf<MONITORINFO>() };
        PInvoke.GetMonitorInfo(monitor, ref monitorInfo);

        var hdcDesktop = PInvoke.GetDC(new());

        var hdcDest = PInvoke.CreateCompatibleDC(hdcDesktop);
        var hBitmap = PInvoke.CreateCompatibleBitmap(hdcDesktop, monitorInfo.rcMonitor.Width, captureHeight);
        PInvoke.SelectObject(hdcDest, hBitmap);

        while (!ct.IsCancellationRequested)
        {
            PInvoke.BitBlt(hdcDest, 0, 0, monitorInfo.rcMonitor.Width, captureHeight,
                hdcDesktop, monitorInfo.rcMonitor.X, monitorInfo.rcMonitor.Y, ROP_CODE.SRCCOPY);

            Image<Bgra32> img;
            unsafe
            {
                BITMAPINFOHEADER bih = new()
                {
                    biWidth = monitorInfo.rcMonitor.Width,
                    biHeight = -captureHeight,
                    biPlanes = 1,
                    biBitCount = 32,
                    biSize = (uint)Unsafe.SizeOf<BITMAPINFOHEADER>(),
                };
                PInvoke.GetDIBits(hdcDest, hBitmap, 0, (uint)captureHeight, null, (BITMAPINFO*)&bih, DIB_USAGE.DIB_RGB_COLORS);

                var bytes = (byte*)NativeMemory.AllocZeroed((nuint)(bih.biSizeImage));
                PInvoke.GetDIBits(hdcDest, hBitmap, 0, (uint)captureHeight, bytes, (BITMAPINFO*)&bih, DIB_USAGE.DIB_RGB_COLORS);

                img = Image.LoadPixelData<Bgra32>(new ReadOnlySpan<byte>(bytes, (int)bih.biSizeImage), monitorInfo.rcMonitor.Width, captureHeight);
                //img.Save("test/captured.png");

                NativeMemory.Free(bytes);
            }

            var anyFound = false;
            for (int x = 0; x < 10; ++x)
                if (TestMask(img, x))
                {
                    anyFound = true;

                    if (TestCount(img, x) is int count)
                        PowerChargeUpdated?.Invoke(count, TimeSpan.Zero);
                    else
                        PowerChargeUpdated?.Invoke(1, TimeSpan.Zero);
                    break;
                }

            if (!anyFound)
                PowerChargeUpdated?.Invoke(0, TimeSpan.Zero);

            await Task.Delay(10, ct).ConfigureAwait(false);
        }
    }, ct);
}
