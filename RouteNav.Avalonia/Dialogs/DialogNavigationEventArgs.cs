namespace NSE.RouteNav.Dialogs;

public class DialogNavigationEventArgs
{
    public DialogNavigationEventArgs(string? routeFrom, Dialog dialogFrom, string? routeTo, Dialog dialogTo)
    {
        RouteFrom = routeFrom;
        DialogFrom = dialogFrom;
        RouteTo = routeTo;
        DialogTo = dialogTo;
    }

    public string? RouteFrom { get; }

    public Dialog DialogFrom { get; }

    public string? RouteTo { get; }

    public Dialog DialogTo { get; }
}
