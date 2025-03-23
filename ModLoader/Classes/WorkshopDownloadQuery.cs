using System;
using Cysharp.Threading.Tasks;

namespace ModLoader.Classes;

public struct WorkshopDownloadQuery
{
    public UniTaskCompletionSource TaskCompletionSource;
    public Action<float> OnProgressUpdate;
}