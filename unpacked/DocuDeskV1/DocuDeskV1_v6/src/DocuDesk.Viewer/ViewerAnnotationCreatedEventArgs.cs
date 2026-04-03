using DocuDesk.Contracts.Viewer;

namespace DocuDesk.Viewer;

public sealed class ViewerAnnotationCreatedEventArgs : EventArgs
{
    public ViewerAnnotationCreatedEventArgs(ViewerAnnotationCreateRequest request)
    {
        Request = request;
    }

    public ViewerAnnotationCreateRequest Request { get; }
}
