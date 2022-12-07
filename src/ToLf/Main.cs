using System.Diagnostics.CodeAnalysis;
using Spectre.Console;

namespace Dot.ToLf;

public sealed class Main : ToolBase<Option>
{
    private                 int          _breakCnt;
    private                 ProgressTask _fileTask;
    private static readonly object       _lockObj = new();
    private                 long         _procedCnt;
    private                 int          _replaceCnt;
    private                 int          _totalCnt;

    public Main(Option opt) : base(opt)
    {
        if (!Directory.Exists(Opt.Path))
            throw new ArgumentException(nameof(Opt.Path), string.Format(Str.PathNotFound, Opt.Path));
    }

    private async ValueTask FileHandle(string file, CancellationToken _)
    {
        ShowMessage(1, 0, 0);

        var tmpFile    = $"{file}.tmp";
        var isReplaced = false;
        var isBin      = false;
        int data;

        await using (var fsr = OpenFileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
            if (Opt.ReadOnly) { //测试，只读模式
                ShowMessage(0, 1, 0);
                return;
            }

            if (fsr is null) {
                ShowMessage(0, 0, 1);
                return;
            }


            await using var fsw = OpenFileStream(tmpFile, FileMode.OpenOrCreate, FileAccess.Write);

            while ((data = fsr.ReadByte()) != -1) {
                switch (data) {
                    case 0x0d when fsr.ReadByte() == 0x0a: // crlf windows
                        fsw.WriteByte(0x0a);
                        isReplaced = true;
                        continue;
                    case 0x0d: //cr macos
                        fsw.WriteByte(0x0a);
                        fsr.Seek(-1, SeekOrigin.Current);
                        isReplaced = true;
                        continue;
                    case 0x00 or 0xff: //非文本文件
                        isBin = true;
                        break;
                    default:
                        fsw.WriteByte((byte)data);
                        continue;
                }

                break;
            }
        }


        if (isReplaced && !isBin) {
            MoveFile(tmpFile, file);
            ShowMessage(0, 1, 0);
        }
        else {
            File.Delete(tmpFile);
            ShowMessage(0, 0, 1);
        }
    }


    private void ShowMessage(int procedCnt, int replaceCnt, int breakCnt)
    {
        lock (_lockObj) {
            _procedCnt  += procedCnt;
            _replaceCnt += replaceCnt;
            _breakCnt   += breakCnt;
            if (procedCnt > 0) _fileTask.Increment(1);
            _fileTask.Description
                = $"{Str.Read}: [green]{_procedCnt}[/]/{_totalCnt}, {Str.Write}: [red]{_replaceCnt}[/], {Str.Break}: [gray]{_breakCnt}[/]";
        }
    }


    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public override async Task Run()
    {
        if (Opt.ReadOnly) AnsiConsole.MarkupLine("[gray]{0}[/]", Str.ExerciseMode);
        IEnumerable<string> fileList;
        await AnsiConsole.Progress()
                         .Columns(new ProgressBarColumn()                                //
                                , new ElapsedTimeColumn()                                //
                                , new PercentageColumn()                                 //
                                , new SpinnerColumn()                                    //
                                , new TaskDescriptionColumn { Alignment = Justify.Left } //
                         )
                         .StartAsync(async ctx => {
                             var taskSearchfile = ctx.AddTask(Str.SearchingFile).IsIndeterminate();
                             _fileTask = ctx.AddTask("-/-", false);
                             fileList  = EnumerateFiles(Opt.Path, Opt.Filter);
                             _totalCnt = fileList.Count();
                             taskSearchfile.IsIndeterminate(false);
                             taskSearchfile.Increment(100);

                             _fileTask.MaxValue = _totalCnt;
                             _fileTask.StartTask();
                             await Parallel.ForEachAsync(fileList, FileHandle);
                         });
    }
}