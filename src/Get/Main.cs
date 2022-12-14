// ReSharper disable ClassNeverInstantiated.Global

using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using NSExt.Extensions;

namespace Dot.Get;

[Description(nameof(Str.DownloadTool))]
[Localization(typeof(Str))]
internal sealed partial class Main : ToolBase<Option>
{
    private const string _PART = "part";

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
                             foreach (var kv in content.Headers) {
                                 table.AddRow(kv.Key, string.Join(Environment.NewLine, kv.Value));
                             }
                         });
        AnsiConsole.Write(table);

        var timer        = DateTime.Now;
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
                             var tParent = ctx.AddTask($"{Str.TotalProgress} {Str.RemainingTime}:").IsIndeterminate();

                             //???????????????????????????????????????
                             if (contentLength == 0) {
                                 await using var nets = await http.GetStreamAsync(Opt.Url);
                                 await using var fs
                                     = new FileStream(mainFilePath, FileMode.CreateNew, FileAccess.Write
                                                    , FileShare.None);
                                 tParent.MaxValue = Opt.BufferSize + 1; //??????????????????????????? ???????????????????????????????????????+1
                                 StreamCopy(nets, fs, x => {
                                     tParent.MaxValue += x;
                                     tParent.Increment(x);
                                 });
                                 tParent.MaxValue = tParent.Value; // ?????????
                                 tParent.IsIndeterminate(false);
                                 tParent.StopTask();
                             }

                             //???????????????????????????????????????
                             else {
                                 tParent.IsIndeterminate(false);
                                 tParent.MaxValue = contentLength;
                                 var chunkSize = contentLength / Opt.ChunkNumbers;

                                 Parallel.For(0, Opt.ChunkNumbers
                                            , new ParallelOptions { MaxDegreeOfParallelism = Opt.MaxParallel } //
                                            , i => {
                                                  var tChild = ctx.AddTask(
                                                      $"{Str.Thread}{i} {Str.RemainingTime}:", maxValue: chunkSize);
                                                  using var getReq   = new HttpRequestMessage(HttpMethod.Get, Opt.Url);
                                                  var       startPos = i * chunkSize;
                                                  var       endPos   = startPos + chunkSize - 1;
                                                  if (i == Opt.ChunkNumbers - 1) {
                                                      endPos += contentLength % chunkSize;
                                                  }

                                                  getReq.Headers.Range = new RangeHeaderValue(startPos, endPos);

                                                  // ReSharper disable once AccessToDisposedClosure
                                                  using var getRsp
                                                      = http.Send(getReq, HttpCompletionOption.ResponseHeadersRead);
                                                  WritePart(getRsp, mainFilePath, i, startPos, endPos, x => {
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

    /// <summary>
    ///     ????????????????????????????????????????????????????????????+??????????????????????????????.
    /// </summary>
    /// <param name="path">???????????????????????????????????????+???????????????????????????.</param>
    /// <param name="file">?????????????????????.</param>
    /// <returns>???????????????????????????????????????.</returns>
    private static string BuildFilePath(string path, string file)
    {
        // path ??????????????????????????????????????????
        if (GetUseablePath(ref path)) {
            return path;
        }

        // ReSharper disable once InvertIf
        if (Directory.Exists(path)) {        //path ???????????????????????????
            path = Path.Combine(path, file); // ??????????????????
            GetUseablePath(ref path);        // ???????????????
            return path;
        }

        // path ??????????????????????????????????????? ???????????????????????????
        return path;
    }

    private static bool GetUseablePath(ref string path)
    {
        var dir  = Path.GetDirectoryName(path);
        var name = Path.GetFileNameWithoutExtension(path);
        var ext  = Path.GetExtension(path);
        var ret  = false;
        #pragma warning disable SA1002
        for (var i = 1;; ++i) {
            #pragma warning restore SA1002
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

    private void WritePart(HttpResponseMessage rsp, string mainFilePath          //
                         , int                 no,  long   startPos, long endPos //
                         , Action<int>         rateHandle)
    {
        Span<byte> buf    = stackalloc byte[Opt.BufferSize];
        using var  stream = rsp.Content.ReadAsStream();
        int        read;
        var        file = $"{mainFilePath}.{no}.{startPos}-{endPos}.{_PART}";
        using var  fs   = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None);
        while ((read = stream.Read(buf)) != 0) {
            fs.Write(read == Opt.BufferSize ? buf : buf[..read]);
            rateHandle(read);
        }
    }
}