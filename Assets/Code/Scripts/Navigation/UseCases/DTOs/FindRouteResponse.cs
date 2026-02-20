
public class FindRouteResponse<T>
{
    public RouteResult<T> RouteResult { get; }
    public bool IsSuccess { get; }

    public FindRouteResponse(RouteResult<T> routeResult)
    {
        RouteResult = routeResult;
        IsSuccess = routeResult.IsSuccess;
    }
}