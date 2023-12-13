// ReSharper disable ClassNeverInstantiated.Global

using System.Net.Http.Headers;
using NSExt.Extensions;

namespace Dot.Get;

[Description(nameof(Ln.多线程下载工具))]
[Localization(typeof(Ln))]
internal sealed class Main : ToolBase<Option>
{
    private const           string _PART      = "part";
    private static readonly Regex  _partRegex = new($"(\\d+)\\.{_PART}", RegexOptions.Compiled);

    protected override async Task CoreAsync()
    {
        using var http          = new HttpClient();
        string    attachment    = default;
        long      contentLength = default;
        var       table         = new Table().AddColumn(Ln.数据标识).AddColumn(Ln.数据内容).AddRow("网络地址", Opt.网络地址);
        await AnsiConsole.Status()
                         .AutoRefresh(true)
                         .Spinner(Spinner.Known.Default)
                         .StartAsync($"{Ln.请求元数据}: {Opt.网络地址}", async _ => {
                             using var headRsp = await http.SendAsync(new HttpRequestMessage(HttpMethod.Head, Opt.网络地址))
                                                           .ConfigureAwait(false);
                             using var content = headRsp.Content;
                             contentLength = content.Headers.ContentLength ?? 0;
                             attachment = content.Headers.ContentDisposition?.FileName ??
                                          Opt.网络地址[(Opt.网络地址.LastIndexOf('/') + 1)..];
                             foreach (var kv in content.Headers) {
                                 #pragma warning disable IDE0058
                                 table.AddRow(kv.Key, string.Join(Environment.NewLine, kv.Value));
                                 #pragma warning restore IDE0058
                             }
                         })
                         .ConfigureAwait(false);
        AnsiConsole.Write(table);

        var timer        = DateTime.UtcNow;
        var mainFilePath = BuildFilePath(Opt.Output, attachment);
        await AnsiConsole.Progress()
                         .Columns(                       //
                             new ProgressBarColumn()     //
                           , new SpinnerColumn()         //
                           , new DownloadedColumn()      //
                           , new TransferSpeedColumn()   //
                           , new PercentageColumn()      //
                           , new TaskDescriptionColumn() //
                           , new RemainingTimeColumn())  //
                         .StartAsync(async ctx => {
                             var tParent = ctx.AddTask($"{Ln.总进度} {Ln.剩余时间}:").IsIndeterminate();

                             // 未知文件长度，单线程下载；
                             if (contentLength == 0) {
                                 await using var nets = await http.GetStreamAsync(Opt.网络地址).ConfigureAwait(false);
                                 await using var fs
                                     = new FileStream(mainFilePath, FileMode.CreateNew, FileAccess.Write
                                                    , FileShare.None);
                                 tParent.MaxValue = Opt.缓冲区大小_千字节 + 1; // 由于文件长度未知， 进度条终点永远至为当前长度+1
                                 StreamCopy(nets, fs, x => {
                                     tParent.MaxValue += x;
                                     tParent.Increment(x);
                                 });
                                 tParent.MaxValue = tParent.Value; // 写完了
                                 _                = tParent.IsIndeterminate(false);
                                 tParent.StopTask();
                             }

                             // 已知文件长度，多线程下载：
                             else {
                                 _                = tParent.IsIndeterminate(false);
                                 tParent.MaxValue = contentLength;
                                 var chunkSize = contentLength / Opt.下载分块数;

                                 async ValueTask BodyActionAsync(int i, CancellationToken cancellationToken)
                                 {
                                     var tChild = ctx.AddTask( //
                                         $"{Ln.线程}{i} {Ln.剩余时间}:", maxValue: chunkSize);
                                     using var getReq   = new HttpRequestMessage(HttpMethod.Get, Opt.网络地址);
                                     var       startPos = i * chunkSize;
                                     var       endPos   = startPos + chunkSize - 1;
                                     if (i == Opt.下载分块数 - 1) {
                                         endPos += contentLength % chunkSize;
                                     }

                                     getReq.Headers.Range = new RangeHeaderValue(startPos, endPos);

                                     // ReSharper disable once AccessToDisposedClosure
                                     using var getRsp = await http
                                                              .SendAsync(
                                                                  getReq, HttpCompletionOption.ResponseHeadersRead
                                                        ,                 cancellationToken)
                                                              .ConfigureAwait(false);
                                     WritePart(getRsp, mainFilePath, i, startPos, endPos, x => {
                                         tChild.Increment(x);
                                         tParent.Increment(x);
                                     });
                                 }

                                 await Parallel.ForAsync(0, Opt.下载分块数
                                                       , new ParallelOptions { MaxDegreeOfParallelism = Opt.最大并发数量 } //
                                                       , BodyActionAsync)
                                               .ConfigureAwait(false);

                                 MergeParts(mainFilePath);
                             }
                         })
                         .ConfigureAwait(false);

        AnsiConsole.MarkupLine($"{Ln.下载完成}, {Ln.累计耗时}: {DateTime.UtcNow - timer}, {Ln.文件保存位置}: {mainFilePath}");
    }

    /// <summary>
    ///     给定一个路径（存在的目录，或者存在的目录+存在或不存在的文件）.
    /// </summary>
    /// <param name="path">存在的目录，或者存在的目录+存在或不存在的文件.</param>
    /// <param name="file">要写入的文件名.</param>
    /// <returns>返回一个可写的文件完整路径.</returns>
    private static string BuildFilePath(string path, string file)
    {
        // path 是一个存在的文件，已追加尾标
        if (GetUsablePath(ref path)) {
            return path;
        }

        // ReSharper disable once InvertIf
        if (Directory.Exists(path)) {        // path 是一个存在的目录。
            path = Path.Combine(path, file); // 构建文件路径
            _    = GetUsablePath(ref path);  // 追加序号。
            return path;
        }

        // path 是一个不存在的目录或者文件 ，视为不存在的文件
        return path;
    }

    private static bool GetUsablePath(ref string path)
    {
        var dir  = Path.GetDirectoryName(path);
        var name = Path.GetFileNameWithoutExtension(path);
        var ext  = Path.GetExtension(path);
        var ret  = false;

        for (var i = 1; /**/; ++i) {
            if (File.Exists(path)) {
                path = Path.Combine(dir!, $"{name}({i}){ext}");
                ret  = true;
                continue;
            }

            break;
        }

        return ret;
    }

    private static void MergeParts(string mainFilePath)
    {
        var files = Directory.GetFiles(                               //
                                 Path.GetDirectoryName(mainFilePath)! //
                               , $"{Path.GetFileName(mainFilePath)}.*.{_PART}", SearchOption.TopDirectoryOnly)
                             .OrderBy(x => x)
                             .ToArray();
        using var fs = File.Create(mainFilePath);
        fs.SetLength(_partRegex.Match(files[^1]).Groups[1].Value.Int64());
        foreach (var file in files) {
            using var fsc = File.OpenRead(file);
            fsc.CopyTo(fs);
            fsc.Close();
            File.Delete(file);
        }
    }

    private void StreamCopy(Stream source, FileStream dest, Action<int> rateHandle)
    {
        Span<byte> buf = stackalloc byte[Opt.缓冲区大小_千字节];
        int        read;
        while ((read = source.Read(buf)) != 0) {
            dest.Write(read == Opt.缓冲区大小_千字节 ? buf : buf[..read]);
            rateHandle(read);
        }
    }

    private void WritePart(HttpResponseMessage rsp, string mainFilePath          //
                         , int                 no,  long   startPos, long endPos //
                         , Action<int>         rateHandle)
    {
        Span<byte> buf    = stackalloc byte[Opt.缓冲区大小_千字节];
        using var  stream = rsp.Content.ReadAsStream();
        int        read;
        var        file = $"{mainFilePath}.{no}.{startPos}-{endPos}.{_PART}";
        using var  fs   = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None);
        while ((read = stream.Read(buf)) != 0) {
            fs.Write(read == Opt.缓冲区大小_千字节 ? buf : buf[..read]);
            rateHandle(read);
        }
    }
}