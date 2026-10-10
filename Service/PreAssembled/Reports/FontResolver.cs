using System;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Fonts;

namespace Service.PreAssembled.Reports
{
    /// <summary>
    /// 报告 PDF 使用的字体族常量。
    /// <para>
    /// PDFsharp 6.x 的 Core 构建<b>不会</b>加载运行环境的系统字体（Windows/Linux 一致行为），
    /// 必须通过 <see cref="IFontResolver"/> 显式提供字节流，因此字体文件随程序集一同分发，
    /// 目录固定为 <c>{AppContext.BaseDirectory}/PreAssembled/Reports/Fonts</c>（见 Service.csproj 的 None Update 项）。
    /// </para>
    /// </summary>
    public static class ReportFonts
    {
        /// <summary>正文等宽族（拉丁字母、数字、标点）。用于还原原报告的固定列宽网格。</summary>
        public const string MonoFamily = "ReportMono";

        /// <summary>等宽粗体。</summary>
        public const string MonoBoldFamily = "ReportMonoBold";

        /// <summary>
        /// 中日韩兜底族。Fira Mono 不含 CJK 字形，项目名称/梁段名称等中文内容必须切到族渲染，
        /// 否则会输出成空白。Builder 侧由 <c>ReportTextWriter.WriteMixed</c> 负责按字符切分。
        /// </summary>
        public const string CjkFamily = "ReportCjk";

        private static readonly object Sync = new object();
        private static bool _initialized;

        /// <summary>
        /// 注册字体解析器。必须在任何 PDF/MigraDoc 操作之前调用一次；
        /// <see cref="GlobalFontSettings.FontResolver"/> 只允许赋值一次，重复赋值会抛异常，故此处加锁 + 幂等。
        /// </summary>
        public static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            lock (Sync)
            {
                if (_initialized)
                {
                    return;
                }

                GlobalFontSettings.FontResolver = new ReportFontResolver();
                _initialized = true;
            }
        }
    }

    /// <summary>
    /// 从随包分发的 TTF 读取字体的解析器。
    /// </summary>
    public sealed class ReportFontResolver : IFontResolver
    {
        /// <summary>字体目录名（相对程序集基目录）。</summary>
        public const string FontFolder = "Fonts";

        private const string MonoRegularFile = "FiraMono-Regular.ttf";
        private const string MonoBoldFile = "FiraMono-Bold.ttf";
        private const string CjkFallbackFile = "CJK-Fallback.ttf";

        // 字体名 -> 文件名。GetFont 的入参就是 ResolveTypeface 返回的 faceName。
        private const string FaceMono = "ReportMono#Regular";
        private const string FaceMonoBold = "ReportMono#Bold";
        private const string FaceCjk = "ReportCjk#Regular";

        private static readonly object Sync = new object();
        private static readonly Dictionary<string, byte[]> Cache = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        /// <summary>字体文件所在目录。</summary>
        public static string FontDirectory
        {
            get { return Path.Combine(AppContext.BaseDirectory, "PreAssembled", "Reports", FontFolder); }
        }

        /// <inheritdoc />
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // 中文兜底族：只有一个字重，粗体请求也复用（避免斜体/粗体族缺失导致回退失败）
            if (string.Equals(familyName, ReportFonts.CjkFamily, StringComparison.OrdinalIgnoreCase))
            {
                return new FontResolverInfo(FaceCjk);
            }

            // 其余一律落到等宽族（报告版式依赖等宽字体还原列网格）
            return new FontResolverInfo(isBold ? FaceMonoBold : FaceMono);
        }

        /// <inheritdoc />
        public byte[]? GetFont(string faceName)
        {
            switch (faceName)
            {
                case FaceMono:
                    return LoadFontFile(MonoRegularFile);
                case FaceMonoBold:
                    return LoadFontFile(MonoBoldFile);
                case FaceCjk:
                    return LoadFontFile(CjkFallbackFile);
                default:
                    return null;
            }
        }

        private static byte[] LoadFontFile(string fileName)
        {
            lock (Sync)
            {
                if (Cache.TryGetValue(fileName, out var cached))
                {
                    return cached;
                }

                var path = Path.Combine(FontDirectory, fileName);
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException(
                        $"报告字体文件缺失：{path}。请确认 Service.csproj 中 PreAssembled/Reports/Fonts/*.ttf 的复制项已生效。",
                        path);
                }

                var bytes = File.ReadAllBytes(path);
                Cache[fileName] = bytes;
                return bytes;
            }
        }
    }
}
