namespace Trading.Web.Client.Pages;

public partial class Counter
{
    protected int CurrentCount { get; private set; }

    protected void IncrementCount()
    {
        CurrentCount++;
    }
}
