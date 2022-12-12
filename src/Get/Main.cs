using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using NSExt.Extensions;

namespace Dot.Get;

[Description(nameof(Str.DownloadTool))]
[Localization(typeof(Str))]
public partial class Main : ToolBase<Option>
{
    private const string _PART = "part";

    /// <summary>
    ///     给定一个路径（存在的目录，或者存在的目录+存在或不存在的文件）
    /// </summary>
    /// <param name="path">存在的目录，或者存在的目录+存在或不存在的文件</param>
    /// <param name="file">要写入的文件名</param>
    /// <returns>返回一个可写的文件完整路径</returns>
    private static string BuildFilePath(string path, string file)
    {
        if (GetUseablePath(ref path))
            // path 是一个存在的文件，已追加尾标
            return path;

        // ReSharper disable once InvertIf
        if (Directory.Exists(path)) {        //path 是一个存在的目录。
            path = Path.Combine(path, file); // 构建文件路径
            GetUseablePath(ref path);        // 追加序号。
            return path;
        }

        // path 是一个不存在的目录或者文件 ，视为不存在的文件
        return path;
    }


    private static bool GetUseablePath(ref string path)
    {
        var dir  = Path.GetDirectoryName(path);
        var name = Path.GetFileNameWithoutExtension(path);
        var ext  = Path.GetExtension(path);
        var ret  = false;
        for (var i = 1;; ++i) {
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
        var files = Directory.GetFiles(Path.GetDirectoryName(mainFilePath)! //
                                     , $"{Path.GetFileName(mainFilePath)}.*.{_PART}", SearchOption.TopDirectoryOnly)
                             .OrderBy(x => x)
                             .ToArray();
        using var fs = File.Create(mainFilePath);
        fs.SetLength(PartRegex().Match(files.Last()).Groups[1].Value.Int64());
        foreach (var file in files) {
            using var fsc = File.OpenRead(file);
            fsc.CopyTo(fs);
            fsc.Close();
            File.Delete(file);
        }
    }

    [GeneratedRegex($"(\\d+)\\.{_PART}")]
    private static partial Regex PartRegex();

    private void StreamCopy(Stream source, Stream dest, Action<int> rateHandle)
    {
        Span<byte> buf = stackalloc byte[Opt.BufferSize];
        int        read;
        while ((read = source.Read(buf)) != 0) {
            dest.Write(read == Opt.BufferSize ? buf : buf[..read]);
            rateHandle(read);
        }
    }

    private void WritePart(HttpResponseMessage rsp,      string mainFilePath //
                          , long                startPos, long   endPos       //
                          , Action<int>         rateHandle)
    {
        Span<byte> buf    = stackalloc byte[Opt.BufferSize];
        using var  stream = rsp.Content.ReadAsStream();
        int        read;
        var        file = $"{mainFilePath}.{startPos}-{endPos}.{_PART}";
        using var  fs   = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None);
        while ((read = stream.Read(buf)) != 0) {
            fs.Write(read == Opt.BufferSize ? buf : buf[..read]);
            rateHandle(read);
        }
    }

    protected override async Task Core()
    {
        using var http = new HttpClient();
        string attachment = default;
        long contentLength = default;
        var table = new Table().AddColumn(Str.DataIdentification).AddColumn(Str.DataContent).AddRow("Url", Opt.Url);
        await AnsiConsole.Status()
                         .AutoRefresh(true)
                         .Spinner(Spinner.Known.Default)
                         .StartAsync($"{Str.RequestMetaData}: {Opt.Url}", async _ => {
                             using var headRsp = await http.SendAsync(new HttpRequestMessage(HttpMethod.Head, Opt.Url));
                             using var content = headRsp.Content;
                             contentLength = content.Headers.ContentLength ?? 0;
                             attachment = content.Headers.ContentDisposition?.FileName ??
                                             Opt.Url[(Opt.Url.LastIndexOf('/') + 1)..];
                             foreach (var kv in content.Headers)
                                 table.AddRow(kv.Key, string.Join(Environment.NewLine, kv.Value));
                         });
        AnsiConsole.Write(table);


        var timer        = DateTime.Now;
        var mainFilePath = BuildFilePath(Opt.Output, attachment);
        await AnsiConsole.Progress()
                         .Columns(new ProgressBarColumn()     //
                                , new SpinnerColumn()         //
                                , new DownloadedColumn()      //
                                , new TransferSpeedColumn()   //
                                , new PercentageColumn()      //
                                , new TaskDescriptionColumn() //
                                , new RemainingTimeColumn()   //
                         )
                         .StartAsync(async ctx => {
                             var tParent = ctx.AddTask($"{Str.TotalProgress} {Str.RemainingTime}:").IsIndeterminate();
                             if (contentLength == 0) //未知文件长度，单线程下载；
                             {
                                 await using var nets = await http.GetStreamAsync(Opt.Url);
                                 await using var fs
                                     = new FileStream(mainFilePath, FileMode.CreateNew, FileAccess.Write
                                                    , FileShare.None);
                                 tParent.MaxValue = Opt.BufferSize + 1; //由于文件长度未知， 进度条终点永远至为当前长度+1
                                 StreamCopy(nets, fs, x => {
                                     tParent.MaxValue += x;
                                     tParent.Increment(x);
                                 });
                                 tParent.MaxValue = tParent.Value; // 写完了
                                 tParent.IsIndeterminate(false);
                                 tParent.StopTask();
                             }
                             else //已知文件长度，多线程下载：
                             {
                                 tParent.IsIndeterminate(false);
                                 tParent.MaxValue = contentLength;
                                 var chunkSize = contentLength / Opt.ChunkNumbers;


                                 Parallel.For(0, Opt.ChunkNumbers
                                            , new ParallelOptions { MaxDegreeOfParallelism = Opt.MaxParallel } //
                                            , i => {
                                                  var tChild = ctx.AddTask(
                                                      $"{Str.Thread}{i} {Str.RemainingTime}:", maxValue: chunkSize);
                                                  using var getReq = new HttpRequestMessage(HttpMethod.Get, Opt.Url);
                                                  var       startPos = i * chunkSize;
                                                  var       endPos = startPos + chunkSize - 1;
                                                  if (i == Opt.ChunkNumbers - 1) endPos += contentLength % chunkSize;
                                                  getReq.Headers.Range = new RangeHeaderValue(startPos, endPos);
                                                  // ReSharper disable once AccessToDisposedClosure
                                                  using var getRsp
                                                      = http.Send(getReq, HttpCompletionOption.ResponseHeadersRead);
                                                  WritePart(getRsp, mainFilePath, startPos, endPos, x => {
                                                      tChild.Increment(x);
                                                      tParent.Increment(x);
                                                  });
                                              });

                                 MergeParts(mainFilePath);
                             }
                         });

        AnsiConsole.MarkupLine(
            $"{Str.DownloadCompleted}, {Str.ElapsedTime}: {DateTime.Now - timer}, {Str.FileSaveLocation}: {mainFilePath}");
    }
}